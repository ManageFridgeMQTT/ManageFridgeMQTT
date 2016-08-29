function isNumberKey(evt)
{
    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57))
    {
        return false;
    }
    return true;
}

function GetDecimalDelimiter(countryCode)
{

switch (countryCode)
{
case 3:
return '#';
case 2:
return ',';
case 1:
return ',';
default:
return '.';
}
}

function GetCommaDelimiter(countryCode)
{

switch (countryCode)
{
case 3:
return '*';
case 2:
return ',';
case 1:
return '.';
default:
return ',';
}

}

function FormatClean(num)
{
var sVal='';
var nVal = num.length;
var sChar='';
var ketqua=num;



try
{
for(c=0;c<nVal;c++)
{
sChar = num.charAt(c);
if (sChar ==0)
{
ketqua = ketqua.substr(1, ketqua.length-1);

if(ketqua.length==1)
{
break;
}
}
else
{
break;
}
}

for(c=0;c<nVal;c++)
{
sChar = ketqua.charAt(c);
nChar = sChar.charCodeAt(0);
if ((nChar >=48) && (nChar <=57))
{
sVal = sVal+ ketqua.charAt(c);
}
}
}
catch (exception) { AlertError("Format Clean",exception); }
return sVal;
}

function FormatNumber(num,countryCode,decimalPlaces)
{
    var minus='';
    var comma='';
    var dec='';
    var preDecimal='';
    var postDecimal='';

try
{
    decimalPlaces = parseInt(decimalPlaces);
    comma = '';//GetCommaDelimiter(countryCode);//lay dau hien thi
    dec =   GetDecimalDelimiter(countryCode);

    if (decimalPlaces < 1)
    {
        dec = '';
    }
    if (num.lastIndexOf("-") == 0)
    {
        minus='-';
    }

    preDecimal = FormatClean(num);

//if (preDecimal.length == decimalPlaces)
//{
//return minus + '' + dec + preDecimal;//gan so mat dinh ''
//}


    var regex  = new RegExp('(-?[0-9]+)([0-9]{3})');

//while(regex.test(preDecimal))
//{
//preDecimal = preDecimal.replace(regex, '$1' + comma + '$2');
//}


}
catch (exception) { 
    AlertError("Format Number",exception); 
}
return minus + preDecimal + postDecimal;
}
