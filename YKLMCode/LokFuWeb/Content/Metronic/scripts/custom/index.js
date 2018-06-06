var Index = function () {
    return {
        //main function
        init: function () {
            App.addResponsiveHandler(function () {
            });
        },
        initCharts: function () {
            if (!jQuery.plot) {
                return;
            }
            function showChartTooltip(x, y, xValue, yValue) {
                $('<div id="tooltip" class="chart-tooltip">'+yValue+'<\/div>').css({
                    position: 'absolute',
                    display: 'none',
                    top: y - 40,
                    left: x - 40,
                    border: '0px solid #ccc',
                    padding: '2px 6px',
                    'background-color': '#fff',
                }).appendTo("body").fadeIn(200);
            }
            var data = [];
            var totalPoints = 250;
            // random data generator for plot charts
            function getRandomData() {
                if (data.length > 0) data = data.slice(1);
                // do a random walk
                while (data.length < totalPoints) {
                    var prev = data.length > 0 ? data[data.length - 1] : 50;
                    var y = prev + Math.random() * 10 - 5;
                    if (y < 0) y = 0;
                    if (y > 100) y = 100;
                    data.push(y);
                }
                // zip the generated y values with the x values
                var res = [];
                for (var i = 0; i < data.length; ++i) res.push([i, data[i]])
                return res;
            }
            function randValue() {
                return (Math.floor(Math.random() * (1 + 50 - 20))) + 10;
            }
            if ($('#site_statistics').size() != 0) {
                $('#site_statistics_loading').hide();
                $('#site_statistics_content').show();
                var plot_statistics = $.plot($("#site_statistics"), 
                    [
                    {
                        data:visitors,
                        lines: {
                            fill: 0.6,
                            lineWidth: 0,
                        },
                        color: ['#f89f9f']
                    },
                    {
                        data: visitors,
                        points: {
                            show: true,
                            fill: true,
                            radius: 5,
                            fillColor: "#f89f9f",
                            lineWidth: 3
                        },
                        color: '#fff',
                        shadowSize: 0
                    },
                    ], 
                    {
                    xaxis: {
                        tickLength: 0,
                        tickDecimals: 0,                        
                        mode: "categories",
                        min: 2,
                        font: {
                            lineHeight: 14,
                            style: "normal",
                            variant: "small-caps",
                            color: "#6F7B8A"
                        }
                    },
                    yaxis: {
                        ticks: 5,
                        tickDecimals: 0,
                        tickColor: "#eee",
                        font: {
                            lineHeight: 14,
                            style: "normal",
                            variant: "small-caps",
                            color: "#6F7B8A"
                        }
                    },
                    grid: {
                        hoverable: true,
                        clickable: true,
                        tickColor: "#eee",
                        borderColor: "#eee",
                        borderWidth: 1
                    }
                });
                var previousPoint = null;
                $("#site_statistics").bind("plothover", function (event, pos, item) {
                    $("#x").text(pos.x.toFixed(2));
                    $("#y").text(pos.y.toFixed(2));
                    if (item) {
                        if (previousPoint != item.dataIndex) {
                            previousPoint = item.dataIndex;
                            $("#tooltip").remove();
                            var x = item.datapoint[0].toFixed(2),
                                y = item.datapoint[1].toFixed(2);
                            showChartTooltip(item.pageX, item.pageY, item.datapoint[0], item.datapoint[1] + ' 元');
                        }
                    } else {
                        $("#tooltip").remove();
                        previousPoint = null;
                    }
                });
            }               
            if ($('#site_activities').size() != 0) {
                //site activities
                var previousPoint2 = null;
                $('#site_activities_loading').hide();
                $('#site_activities_content').show();
                var options = {
                    series: {
                        bars: {
                            show: true,
                            barWidth: 0.9
                        }
                    },
                    grid: {
                        tickColor: "#eee",
                        borderColor: "#eee",
                        hoverable: true,
                        clickable: false,
                        borderWidth: 1
                    },
                    xaxis: {
                        tickLength: 0,
                        tickDecimals: 0,
                        mode: "categories",
                        min: 2,
                        font: {
                            lineHeight: 14,
                            style: "normal",
                            variant: "small-caps",
                            color: "#6F7B8A"
                        }
                    }
                };
                var plot_activities = $.plot($("#site_activities"),
                 [{
                     data: activities,
                     shadowSize: 0,
                     bars: {
                         show: true,
                         lineWidth: 0,
                         fill: true,
                         fillColor: {
                             colors: [{
                                 opacity: 1
                             }, {
                                 opacity: 1
                             }
                             ]
                         }
                     }
                 }]
                 , options);
                $("#site_activities").bind("plothover", function (event, pos, item) {
                    $("#x").text(pos.x.toFixed(2));
                    $("#y").text(pos.y.toFixed(2));
                    if (item) {
                        if (previousPoint2 != item.dataIndex) {
                            previousPoint2 = item.dataIndex;
                            $("#tooltip").remove();
                            var x = item.datapoint[0].toFixed(2),
                                y = item.datapoint[1].toFixed(2);
                            showChartTooltip(item.pageX, item.pageY, item.datapoint[0], item.datapoint[1] + ' 元');
                        }
                    }
                });
                $('#site_activities').bind("mouseleave", function () {
                    $("#tooltip").remove();
                });
            }
        },
        initIntro: function () {
            if ($.cookie('intro_show')) {
                return;
            }
            $.cookie('intro_show', 1);
            setTimeout(function () {
                var unique_id = $.gritter.add({
                    // (string | mandatory) the heading of the notification
                    title: '这里是提示信息!',
                    // (string | mandatory) the text inside the notification
                    text: '欢迎您来到乐富，你知道吗~曾经有个人叫兽!',
                    // (string | optional) the image to display on the left
                    image: '/Content/Metronic/img/avatar1.jpg',
                    // (bool | optional) if you want it to fade out on its own or just sit there
                    sticky: true,
                    // (int | optional) the time you want it to be alive for before fading out
                    time: '',
                    // (string | optional) the class name you want to apply to that specific message
                    class_name: 'my-sticky-class'
                });
                // You can have it return a unique id, this can be used to manually remove it later using
                setTimeout(function () {
                    $.gritter.remove(unique_id, {
                        fade: true,
                        speed: 'slow'
                    });
                }, 13000);
            }, 8000);
        }
    };
}();