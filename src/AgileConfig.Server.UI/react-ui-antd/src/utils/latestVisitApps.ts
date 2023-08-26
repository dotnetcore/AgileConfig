export function saveVisitApp(appId:string, appName:string) {
  console.log('saveVisitApp', appId, appName);
  const apps = localStorage.getItem("latset_visit_apps");
  if(apps) {
    const arr:{appId:string,appName:string}[] = JSON.parse(apps);

    const appIndex = arr.findIndex(x=>x.appId === appId);
    if (appIndex > -1) {
      arr.splice(appIndex, 1);
    }
    if (arr.length >= 4) {
      arr.splice(3, 1);
    } 
    arr.unshift({
      appId,
      appName
    });
    localStorage.setItem('latset_visit_apps', JSON.stringify(arr));
  }
  else{
    localStorage.setItem('latset_visit_apps', JSON.stringify([{
      appId,appName
    }]));
  }
}

export function getVisitApps() {
  const apps = localStorage.getItem("latset_visit_apps");
  if(apps) {
    const arr:{appId:string,appName:string}[] = JSON.parse(apps);
    return arr;
  }
  return [];
}