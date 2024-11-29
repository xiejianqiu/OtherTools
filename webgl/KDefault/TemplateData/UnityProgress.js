function UnityProgress(gameInstance, progress) {
	if (!gameInstance.Module)
		return;
	if (!gameInstance.logo) {
		gameInstance.logo = document.createElement("div");
		gameInstance.logo.className = "logo";
		gameInstance.container.appendChild(gameInstance.logo);
	}
	if (!gameInstance.progress) {    
		gameInstance.progress = document.createElement("div");
		gameInstance.progress.className = "progress";
		gameInstance.progress.empty = document.createElement("div");
		gameInstance.progress.empty.className = "empty";
		gameInstance.progress.appendChild(gameInstance.progress.empty);
		gameInstance.progress.full = document.createElement("div");
		gameInstance.progress.full.className = "full";
		gameInstance.progress.appendChild(gameInstance.progress.full);
		gameInstance.container.appendChild(gameInstance.progress);
	}
	gameInstance.progress.full.style.width = (100 * progress) + "%";
	gameInstance.progress.empty.style.width = (100 * (1 - progress)) + "%";
	if (progress == 1)
		gameInstance.logo.style.display = gameInstance.progress.style.display = "none";
}
function SendMessage(methodName, value) {
	unityInst.SendMessage('GameManagerObject',methodName, value);
}
function ChangeCanvas()
{
	var gameContainer = document.getElementById("gameContainer");
	if(null != gameContainer)
	{
		document.getElementById("gameContainer").style.width = window.innerWidth + "px";
		document.getElementById("gameContainer").style.height = window.innerHeight + "px";
	}
	var canvas = document.getElementById("#canvas");
	if(null != canvas)
	{
	  canvas.style.width = window.innerWidth + "px";
	  canvas.style.height = window.innerHeight + "px";
	}
}
function getQueryString(name) {
	const query = window.location.search.substr(1);
	const params = query.split('&');
	for (let i = 0; i < params.length; i++) {
		const param = params[i].split('=');
		if (param[0] === name) {
			return decodeURIComponent(param[1]);
		}
	}
	return null;
}
function checkSUpportWebAssembly()
{
	return typeof WebAssembly === "object" && typeof WebAssembly.instantiate === "function";
}
function checkSupportWebgl()
{
	const canvas = document.createElement("canvas");
	const gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
	const cp = getQueryString('cp')
	if (!(gl && gl instanceof WebGLRenderingContext && checkSUpportWebAssembly())) {
		if ('360' == cp && window.confirm('您好，该浏览器不支持此游戏，您点击确定可以下载微端？')) {
			var elem = document.createElement('a');
			elem.href = 'https://yxdt.game.keniub.com/weiduan/tgsw2cwbol-360.exe';
			elem.download = 'tgsw2cwbol-360.exe';
			document.body.appendChild(elem);
			elem.click();
			document.body.removeChild(elem);
		}
		if ('360' != cp)
		{
			window.alert('您好，该浏览器不支持此游戏，请更换！');
		}
	}
	var ext = getQueryString('ext')
	if('xl' == ext)
	{
		var scripttag = document.createElement('script');
		scripttag.src = "https://static2-youxi.a.88cdn.com/backend/xl-lib/xl-game.min.2.2.0.js";
		scripttag.type="text/javascript";
		document.body.appendChild(scripttag);
	}
	
	if('lenovo' == cp){
		var scripttag = document.createElement('script');
		scripttag.src = "https://pitf.vgs.lenovo.com.cn/public/about/js/lenovocp_sdk.js";
		scripttag.type="text/javascript";
		document.body.appendChild(scripttag);
	}
	console.log('111浏览器适配检查完毕',ext,cp)
}
function superPay(_cpOrderId, _gameId, _productName, _price, _server_id, _role_id, _role_name, _other) {
	XL_GAME.callApp(
	'superPay',
	{
	cpOrderId: _cpOrderId,
	gameId: _gameId,
	productName: _productName,
	price: _price,
	server_id: _server_id,
	role_id: _role_id,
	role_name: _role_name,
	other: _other
	},
	(res) => console.log('###支付结果',res))
}

//联想支付
function lenovoPay(partner,notifyUrl,outTradeNo,encLenovoId,gameId,extraCommonParam,server,role,lpms,totalFee) {
	var url = 'https://cp.vgs.lenovo.com.cn/pay2?partner=' + partner + '&notifyUrl=' + notifyUrl + '&outTradeNo=' + outTradeNo + '&encLenovoId=' + encLenovoId + '&gameId=' + gameId + '&extraCommonParam=' + extraCommonParam + '&server=' + server + '&role=' + role + '&lpms=' + lpms + "&totalFee=" + totalFee;
	console.log('联想支付url:' + url);
	lenovocp.pay(url, 'pc');
}

//百度支付
function baiduPay(gameId,cpServerId,activeAccount,roleId,cpExtra){
	const postData = {
	    token: '945a7c1579a8aad460a5dcb7bfee927a',
	    action: 'popwindow',
	    data: {
	        gameId: gameId, //百度分配的游戏ID
	        cpServerId: 1, //区服编号
	        activeAccount: activeAccount, //充值金额，单位元，整数数字，暂只支持到元
	        roleId: roleId, //角色ID，非必填
	        cpExtra: cpExtra //补充信息，非必填。在支付完成回调时，将以cp_extra字段原样返回
	    }
	}
	window.top && window.top.window.postMessage(postData, origin);
	console.log('百度支付:gameId:' + gameId + ',cpServerId:' + cpServerId + ",activeAccount:" + activeAccount + ",roleId:" + roleId + ",cpExtra:" + cpExtra);
}

function CopyToClipboard(text) {
	var input = document.createElement('input');
	input.value = text;
	document.body.appendChild(input);
	input.select();
	document.execCommand('copy');
	document.body.removeChild(input);
}
function PasteFromClipboard() {
	var text = '';
	if (navigator.clipboard && window.isSecureContext) {
		navigator.clipboard.readText().then(clipText => {
			text = clipText;
		}).catch(err => {
			console.error('Failed to read clipboard contents: ', err);
		});
	}
	return text;
}
function ListenPasteEvent()
{
	document.addEventListener("paste", function(event) {
		console.log("粘贴事件已触发");
		// 阻止默认粘贴行为
		event.preventDefault();
		// 获取剪贴板数据
		var clipboardData = event.clipboardData || window.clipboardData;
		var pastedData = clipboardData.getData('Text');
		// 执行粘贴后的操作，例如：显示粘贴的文本
		//alert("Pasted data: " + pastedData);
		console.log("Pasted data: " + pastedData);
		//var activeElement = document.activeElement;
		//if(activeElement && 'input'==activeElement.tagName.toLowerCase())
		//{
		//	activeElement.value = pastedData;
		//}
		SendMessage('OnPasteFromClipboard', pastedData);
	}); 
}
function isChineseChar(char) {
    var reg = /[\u4e00-\u9fff]+/g; // 正则表达式，匹配任意中文字符
    return reg.test(char);
}
function isKorean(str) {
    return /^[\u1100-\u11FF]+$/.test(str);
}
function UnityShowInputBox(content) {
	var inputBox =  document.getElementById("customInput");
	if(null == inputBox)
	{
		inputBox = document.createElement('input');
		inputBox.id = 'customInput';
		inputBox.type = 'text';
		inputBox.style.position = 'absolute';
		inputBox.style.left = '0';
		inputBox.style.top = '0';
		inputBox.style.opacity = '0';
		inputBox.style.zIndex = '1000';
		document.body.appendChild(inputBox);
		inputBox.oninput = function(e) {
			//if(isKorean(inputBox.value) || isChineseChar(inputBox.value))
			{
				SendMessage('OnReceiveInput', inputBox.value);
			}
		};
	}
	inputBox.value = content;
	inputBox.focus();
}
function UnityHideInputBox() {
	var inputBox =  document.getElementById("customInput");
	if(null != inputBox)
	{	
		inputBox.value = '';
		inputBox.blur();
	}
}
function ClientInit()
{
	checkSupportWebgl();
	ListenPasteEvent();
}