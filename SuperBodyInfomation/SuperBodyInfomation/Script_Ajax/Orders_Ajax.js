function ExportOrder() {
    var Order = {
        startTime: $("#startTime").val(),
        endTime: $("#endTime").val(),
        phone: $("#phone").val(),
    };
    $.ajax({
        type: 'post',
        url: '/Method/ExportOrder',
        data: Order,
        dataType: 'json', // 服务器返回数据转换成的类型
        success: function (data, responseStatus) {
            if (data != null && data != "") {
                var a = location.origin;
                location.href = location.origin + '/' + data;
            }
            else {
                alert("请选择导出的时间段！");
            }

        }
    });
}