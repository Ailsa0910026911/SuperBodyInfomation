KindEditor.plugin('wordpaster', function(K)
{
	var editor = this, name = 'wordpaster';
	// 点击图标时执行
	editor.clickToolbar(name, function()
	{
		pasterMgr.Paste();
	});
});
