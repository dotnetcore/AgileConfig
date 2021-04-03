import { PageContainer } from '@ant-design/pro-layout';
import React, { useEffect, useState } from 'react';
import {  queryServerNodeStatus } from './service';
import Summary, { summaryProps } from './comps/summary';
import NodeStatiCharts from './comps/nodeClientStatiCharts';

const home: React.FC = () => {
  const [serverNodeStatus, setServerNodeStatus] = useState<summaryProps>({
    clientCount:0,
    nodeStatiInfo: null
  });
  const getServerNodeClientInfos = ()=>{
    queryServerNodeStatus().then((x: {  n:{ address: string},server_status: { clientCount: number } }[]) => {
      let count: number = 0;
      const chartCategorys:string[] = [];
      const chartValues:Number[]=[];
      x?.forEach(item => {
        if (item.server_status) {
          count = count + item.server_status.clientCount;
        }
        chartCategorys.push(item.n.address);
        chartValues.push(item.server_status.clientCount);
      });
      setServerNodeStatus({
        clientCount: count,
        nodeStatiInfo: {
          chartCategorys: chartCategorys,
          chartValues: chartValues
        }
      });
    });
  }
  useEffect(() => {
    getServerNodeClientInfos();
  }, []);
  useEffect(() => {
    const id = setInterval(()=>{
      getServerNodeClientInfos();
    }, 5000);
    return ()=> clearInterval(id);
  }, []);
  return (
    <PageContainer pageHeaderRender={false}>
      <Summary clientCount={serverNodeStatus.clientCount}></Summary>
      {/* { serverNodeStatus.nodeStatiInfo?.chartCategorys.length>1&&<NodeStatiCharts  nodeStatiInfo= { serverNodeStatus.nodeStatiInfo }></NodeStatiCharts>} */}
    </PageContainer>
  );
}
export default home;
