var tinymce_options = {
    script_url :  base_url + 'vendors/tiny_mce/tiny_mce.js',
    content_css: base_url + 'assets/css/global.css?' + (new Date()).getTime(),
    theme_advanced_font_sizes: "9px,10px,12px,13px,14px,16px,18px,20px",
    font_size_style_values : "9px,10px,12px,13px,14px,16px,18px,20px",
    body_class:'product-description',
    theme: 'advanced',
    plugins:'media',
    theme_advanced_toolbar_location: 'top',
    theme_advanced_toolbar_align: 'left',
    theme_advanced_buttons1: 'bold,italic,underline,strikethrough,|,fontselect,fontsizeselect,|,cut,copy,paste,pastetext,|,undo,redo,|,removeformat,|,justifyleft,justifycenter,justifyright,justifyfull,|,forecolor,backcolor,|,sub,sup,|,bullist,numlist,|,link,unlink,|,image,media',
    theme_advanced_buttons2: '',
    theme_advanced_buttons3: '',
    theme_advanced_resizing: false,
    forced_root_block: false,
    force_p_newlines: true,
    remove_linebreaks: false,
    force_br_newlines: true,
    remove_trailing_nbsp: false,
    verify_html: true,    
    invalid_elements: 'script',    
    convert_urls: false,
    relative_urls: false,
    compress: true,
    media_strict: false,
    file_browser_callback: 'openKCFinder'    
};

$(document).ready(function(){    
    tinymce_loadOnDemand();
});

function tinymce_loadOnDemand(){
    $('textarea').not('.not_tinymce').tinymce(tinymce_options);
}

function openKCFinder(field_name, url, type, win){
    
    tinyMCE.activeEditor.windowManager.open({
        file: base_url + 'vendors/kcfinder/browse.php?opener=tinymce&type=' + type + (type=='image'?'s':''),        
        title: 'KCFinder',
        width: 700,
        height: 500,
        resizable: "false",
        inline: true,
        close_previous: "no",
        popup_css: false
    }, {
        window: win,
        input: field_name
    });
    return false;
}