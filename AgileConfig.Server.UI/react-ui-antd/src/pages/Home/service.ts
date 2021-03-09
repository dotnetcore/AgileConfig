import request from "@/utils/request";

export async function queryAppcount() {
    return request('/report/appcount');
}
export async function queryConfigcount() {
    return request('/report/configcount');
}
export async function queryNodecount() {
    return request('/report/nodecount');
}
export async function queryServerNodeStatus() {
    return request('/report/RemoteNodesStatus');
}