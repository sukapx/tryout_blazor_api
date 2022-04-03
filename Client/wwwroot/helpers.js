
function HelperAddOnClick(obj, netObj, method, param) {
  obj.on('click', 
    () => {
    netObj.invokeMethodAsync(method, param);
  });
}
