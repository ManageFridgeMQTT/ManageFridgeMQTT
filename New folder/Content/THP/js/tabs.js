$(document).ready(function(){
	$('#tab-holder li').each(function(){ $('#' + $(this).attr('rel')).hide(); }).click(function(){                  
		$('#'+$('#tab-holder li.selected').removeClass('selected').attr('rel')).hide();
		$('#'+$(this).addClass('selected').attr('rel')).show();
	}).slice(0,1).trigger('click');
});