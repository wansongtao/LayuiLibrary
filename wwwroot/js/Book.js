
//异步请求方法
function ajax(ApiType, FileAddress, JsonData, BookMethod) {
    $.ajax({
        type: ApiType,
        url: FileAddress,
        data: JsonData,
        success: BookMethod,
        error: function (xhr) {
            layer.alert(xhr.statusText, { icon: 5 });
        }
    });
}

//登录
function Login(data) {
    var json = JSON.parse(data);

    if (json.state == 1) {
        setTimeout("top.location.replace('Home/Index')", "1000");
    }
    else {
        layer.alert(json.msg, { icon: 5 });
        $("#submitUser").removeClass("layui-btn-disabled").prop("disabled", false).text("登 入");
    }
}

//layui table 赋值
function Book() {

    layui.use('table', function () {
        var table = layui.table;

        table.render({
            elem: '#demo',  //demo 表格ID
            id: 'BookTable',  //表格重载时可根据该事件ID决定重载哪一个表格
            toolbar: '#toolbarDemo',  //开启头部工具栏
            cols: [[
                { type: 'checkbox', fixed: 'left' },  //width 可不写
                { field: 'BookId', title: 'ID', hide: true },
                { field: 'ISBN', title: '图书编码', fixed: 'left' },
                { field: 'BookName', title: '书名' },
                { field: 'BookType', title: '图书类别' },
                { field: 'Author', title: '作者' },
                { field: 'Price', title: '价格', sort: true },
                { field: 'Publishing', title: '出版社' },
                { fixed: 'right', title: '操作', toolbar: '#barDemo' }  //行工具栏
            ]],
            response: {
                statusName: 'state',    //规定数据状态的字段名称，默认：code
                statusCode: 100        //规定成功的状态码，默认：0
                //,msgName: 'hint'    //规定状态信息的字段名称，默认：msg
                //,countName: 'total' //规定数据总数的字段名称，默认：count
                //,dataName: 'rows' //规定数据列表的字段名称，默认：data
            },
            url: 'Bookview',
            method: 'get',
            parseData: function (res) {  //res 即为原始返回的数据
                return {
                    "state": res.state,  //解析接口状态
                    "msg": res.msg,   //解析提示文本
                    "count": res.count,  //解析数据长度
                    "data": res.data    //解析数据列表(res:json字符串格式)
                };
            },
            even: true,
            page: true,
            limit: 10,
            limits: [5, 10, 15],
            skin: 'line',
            height: 500,
            done: function (res, curr, count) {
                //如果是异步请求数据方式，res即为你接口返回的信息。
                //如果是直接赋值的方式，res即为：{data: [], count: 99} data为当前页数据、count为数据总长度

                wlimit = res.data.length;  //当前页具体数据行数
                wpage = curr;  //当前页码

                if (curr == 1 && count > wlimit) {
                    defLimit = wlimit;
                }
                else if (curr == 1 && count < wlimit) {
                    defLimit = 0;  //获取失败
                }
            }
        });
    });
}

//表格重载
function TableLoad(TableID, pageNumber) {

    layui.use('table', function () {
        var table = layui.table;

        //TableID为table.render事件的ID, pageNumber 重载的页码
        table.reload(TableID, {
            page: {
                curr: pageNumber
            },
            where: {
                empty: "空"
            }
        }, 'data');  //data只重载数据
    });
}

//删除多条数据
function DeleteBook(data) {
    var json = JSON.parse(data);  //将json字符串转换为json对象

    if (json.state == 1) {
        //当前页的数据减去删除的数据后，如果当前页没有数据了，则重载前一页的数据
        if (wlimit - json.count == 0) {
           wpage  = wpage -1;
        }
        var pageNumber = wpage;
        var TableID = 'BookTable';
        TableLoad(TableID, pageNumber);

        //layui.use('table', function () {
        //    var table = layui.table;

        //    //BookTable为table.render事件的ID
        //    table.reload('BookTable', {
        //        page: {
        //            curr: wpage  //当前页
        //        },
        //        where: {
        //            empty: "无意义"
        //        }
        //    }, 'data');  //data只重载数据
        //});

        layer.alert(json.msg, { icon: 6 });
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}

//添加
function BookAdd(data) {
    var json = JSON.parse(data);

    if (json.state == 1) {

        layer.alert(json.msg, { icon: 6 });

        //两秒后关闭添加窗口
        setTimeout(function () {
            var layerindex = parent.layer.getFrameIndex(window.name);
            parent.layer.close(layerindex);
        }, 2000);
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}

//删除一条数据
function DelBookOne(data) {
    var json = JSON.parse(data);

    if (json.state == 1) {
        if (wlimit - 1 == 0) {
            wpage -= 1;
        }

        var pageNumber = wpage;
        var TableID = 'BookTable';
        TableLoad(TableID, pageNumber);

        layer.alert(json.msg, { icon: 6 });
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}

//查询一条数据
function BookOne(data) {
    var json = JSON.parse(data);

    if (json.state == 100) {
        var optionstring;
        cxISBN = json.data[0].ISBN;
        cxBookName = json.data[0].BookName;
        cxBookType = json.data[0].BookType;
        cxAuthor = json.data[0].Author;
        cxPrice = json.data[0].Price;
        cxPublishing = json.data[0].Publishing;

        $("#ISBN").val(cxISBN);
        $("#BookName").val(cxBookName);
        //$("#BookType").val(cxBookType);
        $("#Author").val(cxAuthor);
        $("#Price").val(cxPrice);
        $("#Publishing").val(cxPublishing);

        
        //生成图书类别选择框
        for (var i = 0; i < json.type.length; i++) {
            if (json.type[i] == cxBookType) {
                optionstring += '<option value=\"' + json.type[i] + '\" selected="" >' + json.type[i] + '</option>';
            }
            else {
                optionstring += '<option value=\"' + json.type[i] + '\">' + json.type[i] + '</option>';
            }   
        }

        if (json.type.length > 0) {
            $("#BookType").empty();  //删除子元素
        }
        $("#BookType").append(optionstring);  //添加子元素
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}

//修改
function AlterBooklist(data) {
    var json = JSON.parse(data);

    if (json.state == 1) {
        layer.alert(json.msg, { icon: 6 });

        setTimeout(function () {
            var layerindex = parent.layer.getFrameIndex(window.name);
            parent.layer.close(layerindex);
        }, 2000);
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}

//添加页面图书列表
function SelectType(data) {
    var json = JSON.parse(data);

    if (json.state == 1) {
        var optionstring;

        for (var i = 0; i < json.data.length; i++) {
            optionstring += '<option value = "' + json.data[i].BOOKTYPENAME + '" >' + json.data[i].BOOKTYPENAME + '</option>';
        }

        $("#BookType").append(optionstring);
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}

function leftbooknav(data) {
    var json = JSON.parse(data);

    if (json.state == 1) {
        var navString;

        for (var i = 0; i < json.data.length; i++) {
            if (i == 0) {
                navString = '<dd><a href="'+ json.data[0].BOOKTYPEURL +'" target="BookIframe" >' + json.data[0].BOOKTYPENAME + '</a></dd>';
            }
            else {
                navString += '<dd><a href="' + json.data[i].BOOKTYPEURL +'" target="BookIframe" >' + json.data[i].BOOKTYPENAME + '</a></dd>';
            } 
        }

        if (json.data.length > 0) {
            $("#Booknav").empty();
        }
        $("#Booknav").append(navString);
    }
    else {
        layer.alert(json.msg, { icon: 5 });
    }
}