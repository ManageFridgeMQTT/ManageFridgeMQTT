var colors = [
    "#000080","#FFFF00","#FF0000","#C0C0C0","#FF00FF","#808080","#808000","#800080","#800000","#00FFFF","#00FF00","#008080","#008000","#0000FF","#FFFFFF","#000000",
    "#A3E6B5","#6D908D","#279DF2","#88AD9B","#C06242","#5E9707","#0034C0","#1964FD","#7A2254","#ADB7D1","#4C6608","#237AA2",
    "#9A72A6","#63B161","#D1F4C6","#FD8271","#F2C588","#49E2FF","#C7D3AB","#555971","#78D3E7","#E7D82C","#953CE0","#AF5200","#FD66AA",
    "#BB89AA","#634CC2","#E175FC","#A737BD","#8BA5DB","#FFB9DA","#BE6199","#AFCB88","#965A5D","#F4199B","#0AF5D0","#ABEAEE","#B5E01D",
    "#0789FB","#CBBCCC","#7EB6EA","#169B38","#FD848F","#C10BE6","#3DF76D","#8FEF19","#4700EC","#B9321C","#88A7CB","#27DB90","#7DCA0C",
    "#1C500E","#8D30D2","#902981","#874EF1","#FC60ED","#8F3ED5","#ED6800","#36DD42","#72B4B5","#E06442","#5918A2","#67A075","#F4646F",
    "#6324CF","#2536CB","#0F2618","#D07E21","#15462E","#C706CF","#73A613","#B35644","#DEA10E","#FD2C7F","#73BF23","#A91B89","#9ECC7E",
    "#814DD8","#1A7186","#9FEBE2","#A74818","#33F735","#D2DFB9","#E5B884","#FD0E10","#6F516C","#472734","#C8DED4","#655C05","#9AE6E3",
    "#946F76","#B8EAC4","#DCF1A3","#DF2592","#980544","#EDF269","#23DB4B","#4B6E94","#26FAAA","#E9E51A","#3A1792","#372DA2","#819C6B",
    "#7FC76F","#EF4D10","#374D8A","#0372A0","#76F72F","#5FEAFD","#834B59","#BD53DF","#26C904","#15CE24","#CC7985","#D684AD","#AA547C",
    "#76C63D","#4F6C04","#F18421","#8FDCAF","#76E032","#E449CE","#09726D","#20625B","#147FC3","#757BCE","#5CDA8B","#89B330","#CA0CE9",
    "#4189B3","#719EAE","#52194A","#04D3C1","#ED0FB4","#F0B6D4","#B9DDCB","#EA5F0D","#38678F","#ED4FE8","#931CB4","#14923F","#CB63B0",
    "#41B8F2","#6DF3F7","#5D32C2","#5A6696","#175B3E","#AAA5DF","#D27A74","#D9630D","#0EC916","#5687BA","#C9EC81","#39C9FF","#67B162",
    "#6C66C7","#5F1088","#9CE10C","#6434A1","#28AA12","#109F7E","#9C2A36","#68F7F0","#FA000C","#D6031E","#9A3D35","#A1A84E","#762E36",
    "#1D9C7C","#B6163D","#DDBA01","#93AD42","#E0DA41","#40180F","#C140F8","#3D059D","#C97D49","#430270","#F4DE11","#16353A","#7FFA97",
    "#B2D111","#CEDCB1","#FC5803","#4E3570","#92E12E","#EA4C7F","#A56123","#2340F2","#68EB00","#606E12","#4B35E3","#251CC9","#DE2548",
    "#B9103D","#0B6B8A","#96C3AC","#6FA41F","#740BFC","#45F31D","#5B16FD","#8C131A","#B66278","#C833A2","#4D7C5D","#8D4AD5","#10D92E",
    "#47B2C0","#AA3F2E","#1D8CB7","#364F4F","#F52863","#2CE0D5","#D421B5","#88316A","#CD8C2F","#E9C244","#F7D194","#9CFADE","#5D3C4C",
    "#D2AF72","#2FE801","#448910","#745515","#99C6AA","#9A76B4","#E7A9FD","#E3C6A7","#008951","#111371","#F3031A","#6EEF52","#9F519E",
    "#69A094","#7D7BFF","#FC870E","#E3D63B","#848C40","#888086","#30F76D","#A470A7","#8EFAED","#301FC3","#FDCC06","#443E4C","#90E4C7",
    "#B96EE0","#260CF1","#BDAA20","#1FFCE1","#BFCD8F","#24DC44","#6443E0","#709010","#B2BCC2","#A716FF","#41C922","#DE1F71","#D10623",
    "#4F268E","#C43BF0","#7B9B23","#F13EE4","#2D2B6F","#0AFBFE","#AEAF1C","#31E958","#901194","#B76BBB","#B07B18","#EE82CD","#D4F4F6",
    "#C238E9","#A6B584","#0128DD","#7A9D4B","#603D8E","#5CFE95","#642C84","#69A225","#78125A","#F6C245","#1DB7BF","#1E0EA7","#BA9CAF",
    "#A5BF95","#3191B5","#B520B9","#CD93CF","#2AC268","#3897BE","#13F907","#7B82A5","#2FEB76","#E097BF","#E7AA3E","#19EBC9","#832521",
    "#79569E","#1F65CE","#EC467C","#2733A6","#5D0FA4","#60F00F","#4CFF39","#5EF5D0","#A52CEB","#38E2E6","#1044B0","#603C7F","#6319D6",
    "#426819","#7B29B7","#FEB26A","#0FCBEE","#0BBB29","#ADDCE1","#5CF01C","#963E48","#737E00","#58C038","#190FBF","#E74AC2","#40ADF8",
    "#07EE7D","#5B773B","#744D59","#0ABB83","#06241E","#07DD49","#A1BEC2","#11D077","#7B4100","#DDBF72","#763068","#4C233E","#FB6FCE",
    "#79C672","#8271CF","#0D8631","#31917D","#24DA55","#194230","#AA4C33","#FB1632","#03A387","#99C114","#04B11D","#9C2285","#A59B51",
    "#A7DA3E","#78009F","#46FFD1","#F797BC","#A51D56","#798251","#8312A7","#7E3547","#F21619","#3F671F","#8AEB61","#4D27FC","#BCA8EC",
    "#278DCE","#C2D919","#50C945","#641BF8","#E30721","#4030FA","#9D1067","#AD75F4","#5D1739","#059B52","#2121DE","#044813","#001A15",
    "#72284C","#5D85BC","#656C5B","#56AC6B","#244A90","#220AFB","#632EF3","#430AFA","#22CF93","#F6110D","#EF41D2","#BC7A2C","#F1C0A1",
    "#790725","#952B49","#901320","#C06E12","#585D80","#BC19C7","#0A0F86","#883966","#6656AC","#960B2F","#404F3E","#725C52","#227CF2",
    "#4BB5EF","#7F5D7C","#8BEAEA","#767A25","#C50FE8","#D6C645","#1E208A","#8C91CC","#7B500F","#BC351C","#C69EFB","#8BBB1C","#63316F",
    "#DA2883","#6B8ECB","#9BE155","#FC82FD","#5F972A","#2E7CA8","#E76A06","#369474","#D45CFE","#3AA1E3","#5BB652","#B8CD09","#2383F2",
    "#3E7E14","#99C2B9","#B46397","#8A111F","#3E2D18","#2398C6","#34B556","#5B2FE8","#0E3170","#E0B00C","#69A905","#F4E210","#BDAF24",
    "#14BF15","#2B7DC8","#D0D029","#C4B724","#71F657","#6FCBB9","#A0D66D","#A4CF19","#67D6F2","#F9E985","#564E13","#2A3D69","#B360F2",
    "#34BA0B","#593C24","#606014","#3559C3","#85A845","#A79C75","#834BDA","#58F905","#D90D5A","#6536A2","#4CF0E1","#C63770","#CB3FE0",
    "#4E8F14","#FDA6CD","#EE8420","#EB87F1","#A865B2","#893605","#9E7DCE","#3F9DD4","#B86288","#4EAC6F","#4BAC8A","#53CB6B","#6F96A9",
    "#97E9C0","#CA340A","#8BA7BD","#B141AF","#585FEE","#ED9A3E","#9E314A","#CE67B8","#072FB5","#CB7E72","#F1EC9A","#2BEC86","#3FAEF5",
    "#F10910","#78B72D","#942F3B","#7DCE1A","#0027F2","#BB4CFF","#C1FD9D","#3B7575","#A805E6","#E649A7","#BC72CD","#FD539C","#7B6A50",
    "#E51BD7","#7BDC7D","#24B590","#BAFA2B","#88F240","#14EBC8","#2A4FF1","#D08E38","#995F37","#E863E7","#D2DFE8","#3995C9","#5D68E9",
    "#9E6213","#19FA0D","#2899C9","#B7F791","#DE046F","#152A7B","#29EC77","#3A9AD8","#829158","#2169D2","#F36A00","#2D4117","#993B65",
    "#104BCD","#F92E7B","#9709A2","#B6997F","#3C7C89","#ABF7BF","#FD540D","#1CDCB6","#8FF876","#2BC38C","#8D418C","#F1E703","#AC073F",
    "#1717C6","#E72F54","#2521F1","#AB65E0","#21FBD8","#BF550A","#BEF3FF","#BBEE54","#B9B555","#FA53F1","#2C8FDE","#3B0315","#64C067",
    "#E698BD","#D29CFB","#C93AE2","#5D6838","#6EB062","#29045B","#D136EB","#04EA61","#32F972","#0911D1","#095FC0","#EEBC70","#97E2F8",
    "#2A8C61","#61D1EC","#C91DF3","#C5B8E9","#9E8329","#973D4C","#0FAED4","#CF168D","#EBC873","#C280CD","#5156CD","#E25D69","#89116C",
    "#C7873A","#122942","#A9204E","#FB10D6","#9B7D4D","#699784","#B22574","#17A832","#E7AA23","#538249","#C49C72","#24954A","#8129D0",
    "#2E4B4B","#A85414","#22D67E","#B95C7E","#154762","#15190E","#1C242A","#4E7518","#C559C5","#DF1186","#8AB354","#755C47","#5AD523",
    "#8BA966","#B76232","#F92FDA","#B9F8F6","#12CF5F","#C46E81","#1B1F86","#5995F5","#456368","#238E48","#6B8F29","#4ECA88","#FA595A",
    "#A8269A","#44756A","#93C84B","#DCA338","#DA3000","#31B2EC","#5CBEEC","#280864","#DA077E","#E64BB4","#A54D5C","#6E16EB","#6739CB",
    "#EFD9D1","#79C60B","#BEC0A7","#68EFFC","#9B22A3","#50C3D1","#65E8E3","#1F7EFE","#01817D","#886A97","#98AD9F","#84FA58","#6DF4A4",
    "#63F0FC","#319C72","#5C9321","#AECAE9","#F8B526","#B0E97E","#75CBCB","#2E786B","#725881","#6A619C","#FB6F2C","#CFB826","#4610A6",
    "#EB8363","#1A6A46","#E9193F","#945C85","#12ED3A","#135DB5","#E033FD","#8909E8","#938A33","#AA0409","#E90F9D","#8A65C8","#F136C7",
    "#2C262B","#CBED24","#25102F","#1D1114","#7FFBDF","#655D7C","#06DC4D","#3C9399","#707491","#F17F02","#E3A922","#1460D0","#1DFE0F",
    "#DCBDDC","#00FE63","#031556","#94D975","#37682D","#0BB77D","#35B9AE","#C21E48","#91AA58","#43CA3B","#2020DE","#8408FC","#1EC62C",
    "#46B25B","#286082","#321D22","#D50CFB","#AC9D5F","#8AA306","#FD1448","#31E820","#5C110D","#4FDC01","#896BFD","#F74C85","#BC2FB5",
    "#6A6ED0","#A1D397","#4B2356","#1E2BB6","#A39D95","#428151","#15CBD6","#7613E1","#397F05","#4BB7D3","#8074FF","#02563C","#997045",
    "#CD9DB8","#1AF049","#B6E528","#C84D58","#8AB87B","#ABE376","#4D153E","#7C786F","#4A1043","#B25F17","#6DC33F","#0F697D","#9E231E",
    "#ECBBCF","#D18219","#C69B61","#5D93F2","#F60E19","#72C890","#05FFD0","#5F63CB","#A0F6B5","#8E652A","#508513","#245E50","#1A3280",
    "#DE9EE6","#F30484","#DD232E","#9180B8","#C98DEB","#79C787","#B8F689","#664D60","#106197","#FE2070","#AD5647","#6C3291","#8192AB"];

//////var colors_16 =   ["Navy",    "Yellow",   "Red",      "Silver",   "Fuchsia",  "Gray",     "Olive",    "Purple",   "Maroon",   "Aqua",     "Lime",     "Teal",     "Green",    "Blue",    "White",     "Black"];
////var colors_16 =     ["#000080", "#FFFF00",  "#FF0000",  "#C0C0C0",  "#FF00FF",  "#808080",  "#808000",  "#800080",  "#800000",  "#00FFFF",  "#00FF00",  "#008080",  "#008000",  "#0000FF",  "#FFFFFF",  "#000000"];