wx.config({
    debug: false,
    appId: appId,
    timestamp: timestamp,
    nonceStr: nonceStr,
    signature: signature,
    jsApiList: [
        'onMenuShareTimeline',
        'onMenuShareAppMessage',
        'onMenuShareQQ',
        'onMenuShareWeibo'
    ]
});
wx.ready(function () {
    var PshareData = shareData;
    //朋友
    wx.onMenuShareAppMessage(PshareData);
    //朋友圈
    wx.onMenuShareTimeline(PshareData);
    //QQ
    wx.onMenuShareQQ(PshareData);
    //微博
    wx.onMenuShareWeibo(PshareData);
})