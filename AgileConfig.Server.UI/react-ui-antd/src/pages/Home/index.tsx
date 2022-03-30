import { PageContainer } from '@ant-design/pro-layout';
import React, { useEffect, useState } from 'react';
import { queryServerNodeStatus } from './service';
import Summary, { summaryProps } from './comps/summary';
import { queryNodes, addNode } from './../Nodes/service'
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { NodeItem } from './../Nodes/data';
import { message, Modal } from 'antd';
import { getIntl, getLocale } from 'umi';
import LatestVisitApps from './comps/latestVisitApps';
const { confirm } = Modal;

const handleAdd = async (fields: NodeItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'adding'
  }));
  try {
    const result = await addNode({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'add_success'
      }));
    } else {
      message.error(intl.formatMessage({
        id: 'add_fail'
      }));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'add_fail'
    }));
    return false;
  }
};

const home: React.FC = () => {
  const [serverNodeStatus, setServerNodeStatus] = useState<summaryProps>({
    clientCount: 0,
    nodeCount: 0,
    nodeOnCount: 0
  });
  const getServerNodeClientInfos = () => {
    queryServerNodeStatus().then((x: { n: { address: string, status: number }, server_status: { clientCount: number } }[]) => {
      let clientCount: number = 0;
      x?.forEach(item => {
        if (item.server_status) {
          clientCount = clientCount + item.server_status.clientCount;
        }
      });
      let nodeCount: number = 0;
      nodeCount = x.length;
      let nodeOnCount: number = 0;
      nodeOnCount = x.filter(n => n.n.status === 1).length;
      console.log('nodeOnCount', nodeOnCount);
      setServerNodeStatus({
        clientCount: clientCount,
        nodeCount: nodeCount,
        nodeOnCount: nodeOnCount
      });
    });
  }

  const anyServerNode = async () => {
    const nodes = await queryNodes();
    return nodes.data.length > 0;
  }

  useEffect(() => {
    anyServerNode().then(data => {
      if (!data) {
        console.log('No nodes plz add one !');
        let confirmMsg = `节点列表为空，是否添加当前节点【${window.location.origin}】到节点列表？`;
        confirm({
          icon: <ExclamationCircleOutlined />,
          content: confirmMsg,
          onOk() {
            const origin = window.location.origin;
            console.log(` try add ${origin} to node list .`);
            const node: NodeItem = {
              address: origin,
              remark: '控制台节点',
              status: 0
            };
            handleAdd(node);
          },
          onCancel() {
          },
          okText: '马上添加'
        })
      }
    });
  }, []);
  useEffect(() => {
    getServerNodeClientInfos();
    const id = setInterval(() => {
      getServerNodeClientInfos();
    }, 5000);
    return () => clearInterval(id);
  }, []);
  return (
    <PageContainer pageHeaderRender={false}>
      <Summary clientCount={serverNodeStatus.clientCount} nodeCount={serverNodeStatus.nodeCount} nodeOnCount={serverNodeStatus.nodeOnCount}></Summary>
      <LatestVisitApps />
    </PageContainer>
  );
}
export default home;

