import { AppstoreOutlined, DatabaseOutlined, HomeFilled, ShrinkOutlined, TableOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import React, { useEffect, useState } from 'react';
import styles from './index.less';
import { queryAppcount, queryConfigcount, queryNodecount, queryServerNodeStatus } from './service';
import { history } from 'umi';
import * as echarts from 'echarts';

export type itemInfoProps = {
  type: string,
  icon: JSX.Element,
  count: number,
  link: string
}

const ItemInfo: React.FC<itemInfoProps> = (props) => {
  const itemColor = () => {
    if (props.type == 'node') {
      return '#1890ff';
    }
    if (props.type == 'app') {
      return '#faad14';
    }
    if (props.type == 'config') {
      return '#13c2c2';
    }
    if (props.type == 'client') {
      return '#52c41a';
    }

    return '';
  }
  const itemName = () => {
    if (props.type == 'node') {
      return '节点';
    }
    if (props.type == 'app') {
      return '应用';
    }
    if (props.type == 'config') {
      return '配置';
    }
    if (props.type == 'client') {
      return '客户端';
    }

    return '';
  }
  return (
    <div className={styles.item} style={{ backgroundColor: itemColor() }} onClick={()=>{
      const link = props.link;
      if (link) {
        history.push(link);
      }
    }}>
      <div>
        <div className={styles.count}>{props.count}</div>
        <div className={styles.name}>
          {
            itemName()
          }
        </div>
      </div>
      <div className={styles.icon}>
        {
          props.icon
        }
      </div>
    </div>

  );
}

export type summaryProps = {
  clientCount?: number,
  nodeStatiInfo?: any
}

const Summary: React.FC<summaryProps> = (props) => {
  const [appCount, setAppCount] = useState<number>(0);
  const [configCount, setConfigCount] = useState<number>(0);
  const [nodeCount, setNodecount] = useState<number>(0);

  useEffect(() => {
    queryNodecount().then(x => setNodecount(x));
  }, []);
  useEffect(() => {
    queryConfigcount().then(x => setConfigCount(x));
  }, []);
  useEffect(() => {
    queryAppcount().then(x => setAppCount(x));
  }, []);

  return (
    <div className={styles.panel}>
      <div className={styles.summary}>
      <ItemInfo count={nodeCount} type="node" link="/node" icon={<DatabaseOutlined ></DatabaseOutlined>}></ItemInfo>
      <ItemInfo count={appCount} type="app" link="/app" icon={<AppstoreOutlined ></AppstoreOutlined>}></ItemInfo>
      <ItemInfo count={configCount} type="config" link="/app" icon={<TableOutlined ></TableOutlined>}></ItemInfo>
      <ItemInfo count={props.clientCount??0} type="client" link="/client" icon={<ShrinkOutlined ></ShrinkOutlined>}></ItemInfo>
      </div>
    </div>
  );
}
const NodeStatiCharts: React.FC<summaryProps> = (props)=> {
  useEffect(() => {
    const el = document.getElementById('nodeClientsChart');
      if (el) {
        const myChart = echarts.init(el);
        const option = {
          legend: {
            data: ['客户端数量']
          },
          tooltip: {
            trigger: 'axis',
            axisPointer: {            // 坐标轴指示器，坐标轴触发有效
                type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
            }
          },
          title: {
          },
          xAxis: {
              type: 'category',
              data: props.nodeStatiInfo?.chartCategorys??[]
          },
          yAxis: {
              type: 'value'
          },
          series: [{
              name:'客户端数量',
              data: props.nodeStatiInfo?.chartValues??[],
              type: 'bar'
          }]
        };
        myChart.setOption(option);
      }
  }, [props.nodeStatiInfo]);
  return(
    <div className={styles.panel}>
      <div id="nodeClientsChart" className={styles.node_clients} >
      </div>
    </div>

  );
}
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
      {
        <NodeStatiCharts  nodeStatiInfo= { serverNodeStatus.nodeStatiInfo }></NodeStatiCharts>
      }
    </PageContainer>
  );
}
export default home;
