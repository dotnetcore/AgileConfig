import { AppstoreOutlined, DatabaseOutlined, HomeFilled, ShrinkOutlined, TableOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import React, { useEffect, useState } from 'react';
import styles from './index.less';
import { queryAppcount, queryConfigcount, queryNodecount, queryServerNodeStatus } from './service';

export type itemInfoProps = {
  type: string,
  icon: JSX.Element,
  count: number
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
    <div className={styles.item} style={{ backgroundColor: itemColor() }}>
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
const Summary: React.FC = () => {
  const [appCount, setAppCount] = useState<number>(0);
  const [configCount, setConfigCount] = useState<number>(0);
  const [nodeCount, setNodecount] = useState<number>(0);
  const [clientCount, setClientCount] = useState<number>(0);

  useEffect(() => {
    queryNodecount().then(x => setNodecount(x));
  }, []);
  useEffect(() => {
    queryConfigcount().then(x => setConfigCount(x));
  }, []);
  useEffect(() => {
    queryAppcount().then(x => setAppCount(x));
  }, []);
  useEffect(() => {
    queryServerNodeStatus().then((x: { server_status: { clientCount: number } }[]) => {
      let count: number = 0;
      x.forEach(item => {
        count = count + item.server_status.clientCount;
      });
      setClientCount(count);
    });
  }, []);
  return (
    <div className={styles.summary}>
      <ItemInfo count={nodeCount} type="node" icon={<DatabaseOutlined ></DatabaseOutlined>}></ItemInfo>
      <ItemInfo count={appCount} type="app" icon={<AppstoreOutlined ></AppstoreOutlined>}></ItemInfo>
      <ItemInfo count={configCount} type="config" icon={<TableOutlined ></TableOutlined>}></ItemInfo>
      <ItemInfo count={clientCount} type="client" icon={<ShrinkOutlined ></ShrinkOutlined>}></ItemInfo>

    </div>
  );
}
const home: React.FC = () => {
  return (
    <PageContainer pageHeaderRender={false}>
      <Summary></Summary>
    </PageContainer>
  );
}
export default home;
