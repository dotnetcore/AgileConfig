import { PageContainer } from '@ant-design/pro-layout';
import React, { useEffect, useState } from 'react';
import { queryServerNodeStatus } from './service';
import Summary, { summaryProps } from './comps/summary';
import { queryNodes, addNode } from './../Nodes/service'
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { NodeItem } from './../Nodes/data';
import { message, Modal } from 'antd';
import { hasFunction } from '@/utils/authority';
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
    // Only prompt to add current domain as a node if user has NODE_ADD permission
    if (!hasFunction('NODE_ADD')) {
      return; // skip check & prompt entirely
    }
    anyServerNode().then(exists => {
      if (exists) return;
      const intl = getIntl(getLocale());
      const confirmMsg =
        intl.formatMessage({ id: 'pages.home.empty_node_confirm' }) +
        `【${window.location.origin}】` +
        intl.formatMessage({ id: 'pages.home.to_node_list' });
      confirm({
        icon: <ExclamationCircleOutlined />,
        content: confirmMsg,
        onOk() {
          const origin = window.location.origin;
          const node: NodeItem = {
            address: origin,
            remark: intl.formatMessage({ id: 'pages.home.consoleNode' }),
            status: 0,
          };
            handleAdd(node);
        },
        okText: intl.formatMessage({ id: 'pages.home.add_now' }),
      });
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

