Highcharts.getOptions().plotOptions.pie.colors = (function () {
    var colors = [];
    var base = Highcharts.getOptions().colors[0];
    var i;
    colors.push('#0213EA');
    colors.push('#0FF608');
    colors.push('#FFFD01');
    colors.push('#FEB101');
    colors.push('#FF1200');
    return colors;
}());
function RenderChart(chartID, data, chartType) {
    $('#' + chartID).highcharts({
        chart: {
            type: chartType
        },
        title: {
            text: data.chartName,
            align: 'left',
            style: {
                color: '#4e576a',
                "fontSize": "20px",
                "font-family": "inherit",
                "font-weight": "500"
            },
        },
        xAxis: {
            categories: data.listColumns
        },
        yAxis: {
            title: {
                text: data.YName
            }
        },
        series: data.listSeries
    });
}

function RenderLine(chartID, data, chartType) {
    $('#' + chartID).highcharts({
        title: {
            text: data.chartName,
            align: 'left',
            style: {
                color: '#4e576a',
                "fontSize": "20px",
                "font-family": "inherit",
                "font-weight": "500"
            },
        },
        xAxis: {
            categories: data.listColumns
        },
        yAxis: {
            title: {
                text: data.YName
            }
        },
        series: data.listSeries
    });
}