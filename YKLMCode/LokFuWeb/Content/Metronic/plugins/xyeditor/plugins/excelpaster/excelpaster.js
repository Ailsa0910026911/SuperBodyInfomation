KindEditor.plugin('excelpaster', function(K)
{
	var editor = this, name = 'excelpaster';
	// 点击图标时执行
	editor.clickToolbar(name, function()
	{
		pasterMgr.PasteExcel();
	});
});
