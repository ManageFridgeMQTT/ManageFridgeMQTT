DXCommon = {};
DXCommon.CurrentDemoGroupKey = "";
DXCommon.CurrentDemoKey = "";
DXCommon.CurrentThemeCookieKey = "theme";
DXCommon.CurrentLanguageCookieKey = "language";
DXCommon.CodeLoaded = false;
DXCommon.InitDemoView = function(){
    DXCommon.CodeLoaded = TabControl.GetActiveTab().name == "Code";
    DXCommon.DemoViewChanged();
}

DXCommon.ShowCodeInNewWindow = function(demoTitle, codeTitle){
    var codeWnd = window.open("", "_blank", "status=0,toolbar=0,scrollbars=1,resizable=1,top=74,left=74,width=" + (screen.availWidth - 150) + ",height=" + (screen.availHeight - 150));
    codeWnd.document.open();
    codeWnd.document.write("<html><head><title>");
    codeWnd.document.write(demoTitle + " - " + codeTitle);
    codeWnd.document.write("</title>");
    var links = document.getElementsByTagName("link");
    for(var i = 0; i < links.length; i++){
        if(links[i].href && links[i].href.indexOf("CodeFormatter.css") > -1){
            codeWnd.document.write("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + links[i].href + "\" />");
            break;
        }
    }
    codeWnd.document.write("</head><body><code>");
    var codeGroup = CodeNavBar.GetGroupByName(codeTitle);
    codeWnd.document.write(document.getElementById("CodeBlock" + codeGroup.index).innerHTML);
    codeWnd.document.write("</code></body></html>");
    codeWnd.document.close();
}
DXCommon.ShowThemeSelector = function(){
    ThemeSelectorPopup.Show();
}
DXCommon.GetThemeSelectorButton = function() {
    return document.getElementById("ThemeSelectorButton");
};
DXCommon.ThemeSelectorPopupPopUp = function(){
    DXCommon.GetThemeSelectorButton().className += " SelectedButton";
}
DXCommon.ThemeSelectorPopupCloseUp = function(){
    var b = DXCommon.GetThemeSelectorButton();
    b.className = b.className.replace("SelectedButton", "");
}
DXCommon.ShowLanguageSelector = function () {
    LanguageSelectorPopup.Show();
}
DXCommon.GetLanguageSelectorButton = function () {
    return document.getElementById("LanguageSelectorButton");
};
DXCommon.LanguageSelectorPopupPopUp = function () {
    DXCommon.GetLanguageSelectorButton().className += " SelectedButton";
}
DXCommon.LanguageSelectorPopupCloseUp = function () {
    var b = DXCommon.GetLanguageSelectorButton();
    b.className = b.className.replace("SelectedButton", "");
}

DXCommon.SetCurrentTheme = function (theme) {
    ThemeSelectorPopup.Hide();
    ASPxClientUtils.SetCookie(DXCommon.CurrentThemeCookieKey, theme);
    if (document.forms[0] && (!document.forms[0].onsubmit || document.forms[0].onsubmit.toString().indexOf("Sys.Mvc.AsyncForm") == -1)) {
        // for export purposes
        var eventTarget = document.getElementById("__EVENTTARGET");
        if (eventTarget)
            eventTarget.value = "";
        var eventArgument = document.getElementById("__EVENTARGUMENT");
        if (eventArgument)
            eventArgument.value = "";
        window.location.reload();
    } else {
        window.location.reload();
    }
}
DXCommon.SetCurrentLanguage = function (language) {
    LanguageSelectorPopup.Hide();
    ASPxClientUtils.SetCookie(DXCommon.CurrentLanguageCookieKey, language);
    if (document.forms[0] && (!document.forms[0].onsubmit || document.forms[0].onsubmit.toString().indexOf("Sys.Mvc.AsyncForm") == -1)) {
        // for export purposes
        var eventTarget = document.getElementById("__EVENTTARGET");
        if (eventTarget)
            eventTarget.value = "";
        var eventArgument = document.getElementById("__EVENTARGUMENT");
        if (eventArgument)
            eventArgument.value = "";
        window.location.reload();
    } else {
        window.location.reload();
    }
}

DXCommon.CodeNavBar_HeaderClick = function(s, e) {
    var source = _aspxGetEventSource(e.htmlEvent);
    var tag = source.tagName;
    e.cancel = tag != "A" && tag != "IMG";
}
DXCommon.ShowScreenshotWindow = function(evt, link){
    DXCommon.ShowScreenshot(link.href); 
    evt.cancelBubble = true; 
    return false;
}

DXCommon.ShowScreenshot = function(src){
    var screenLeft = document.all && !document.opera ? window.screenLeft : window.screenX;
	var screenWidth = screen.availWidth;
	var screenHeight = screen.availHeight;
    var zeroX = Math.floor((screenLeft < 0 ? 0 : screenLeft) / screenWidth) * screenWidth;
    
	var windowWidth = 475;
	var windowHeight = 325;
	var windowX = parseInt((screenWidth - windowWidth) / 2);
	var windowY = parseInt((screenHeight - windowHeight) / 2);
	if(windowX + windowWidth > screenWidth)
		windowX = 0;
	if(windowY + windowHeight > screenHeight)
		windowY = 0;

    windowX += zeroX;

	var popupwnd = window.open(src,'_blank',"left=" + windowX + ",top=" + windowY + ",width=" + windowWidth + ",height=" + windowHeight + ", scrollbars=no, resizable=no", true);
	if (popupwnd != null && popupwnd.document != null && popupwnd.document.body != null) {
	    popupwnd.document.body.style.margin = "0px"; 
    }
}

DXEventMonitor = {
    TimerId: -1,
    PendingUpdates: [ ],
    
    Trace: function(sender, e, eventName) {
        var checkbox = document.getElementById("EventCheck" + eventName);
        if(!checkbox.checked) return;
        
        var self = DXEventMonitor;
        var name = sender.name;
        var lastSeparator = name.lastIndexOf("_");
        if(lastSeparator > -1)
            name = name.substr(lastSeparator + 1);

        var builder = ["<table cellspacing=0 cellpadding=0 border=0>"];
        self.BuildTraceRowHtml(builder, "Sender", name, 100);
        self.BuildTraceRowHtml(builder, "EventType", eventName.replace(/_/g, " "));
        var count = 0;
        for(var child in e) {
            if (typeof e[child] == "function") continue;
            var childValue = e[child];
            if (typeof e[child] == "object" && e[child] && e[child].name)
                childValue = "[name: '" + e[child].name + "']";
            self.BuildTraceRowHtml(builder, count ? "" : "Arguments", child + " = " + childValue);
            count++;
        }                
        builder.push("</table><br />");
        
        self.PendingUpdates.unshift(builder.join(""));
        if(self.TimerId < 0)
            self.TimerId = window.setTimeout(self.Update, 0);
    },
    
    BuildTraceRowHtml: function(builder, name, text, width) {
        builder.push("<tr><td valign=top");
        if(width)
            builder.push(" style='width: ", width, "px'");
        builder.push(">");
        if(name)
            builder.push("<b>", name, ":</b>");        
        builder.push("</td><td valign=top>", text, "</td></tr>");
    },
    
    GetLogElement: function() {
        return document.getElementById("EventLog")
    },
    
    Update: function() {                    
        var self = DXEventMonitor;
        var element = self.GetLogElement();
        
        element.innerHTML = self.PendingUpdates.join("") + element.innerHTML;
        self.TimerId = -1;
        self.PendingUpdates = [ ];                
    },
    
    Clear: function() {
       DXEventMonitor.GetLogElement().innerHTML = ""; 
    }
};
