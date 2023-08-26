import { AppstoreOutlined, CloudOutlined, DatabaseOutlined, ShrinkOutlined, TableOutlined } from "@ant-design/icons";
import React, { useEffect, useState } from "react";
import { queryAppcount, queryConfigcount, queryServiceCount } from "../service";
import ItemInfo from "./itemInfo";
import styles from '../index.less';
import { Col, Row } from "antd";

export type summaryProps = {
  clientCount?: number,
  nodeCount?: number,
  nodeOnCount?: number
}

const Summary: React.FC<summaryProps> = (props) => {
  const [appCount, setAppCount] = useState<number>(0);
  const [configCount, setConfigCount] = useState<number>(0);
  const [serviceCount, setServiceCount] = useState<number>(0);
  const [serviceOnCount, setServiceOnCount] = useState<number>(0);

  const getServiceCount = ()=>{
    queryServiceCount().then(x => {
      setServiceCount(x.serviceCount);
      setServiceOnCount(x.serviceOnCount);
    });
  }

  useEffect(() => {
    queryConfigcount().then(x => setConfigCount(x));
  }, []);
  useEffect(() => {
    queryAppcount().then(x => setAppCount(x));
  }, []);
  useEffect(() => {
    getServiceCount();
    const id = setInterval(() => {
      getServiceCount();
    }, 5000);
    return () => clearInterval(id);
  }, []);
  return (
    <div className={styles.panel}>
      <Row gutter={16} className={styles.summary}>
        <Col flex={1}>
          <ItemInfo count={props.nodeOnCount + '/' + props.nodeCount} type="node" link="/node" icon={<DatabaseOutlined ></DatabaseOutlined>}></ItemInfo>
        </Col>
        <Col flex={1}>
          <ItemInfo count={appCount + ''} type="app" link="/app" icon={<AppstoreOutlined ></AppstoreOutlined>}></ItemInfo>
        </Col>
        <Col flex={1}>
          <ItemInfo count={configCount + ''} type="config" link="/app" icon={<TableOutlined ></TableOutlined>}></ItemInfo>
        </Col>
        <Col flex={1}>
          <ItemInfo count={props.clientCount + ''} type="client" link="/client" icon={<ShrinkOutlined ></ShrinkOutlined>}></ItemInfo>
        </Col>
        <Col flex={1}>
          <ItemInfo count={serviceOnCount + '/' + serviceCount} type="service" link="/service" icon={<CloudOutlined ></CloudOutlined>}></ItemInfo>
        </Col>
      </Row>
    </div>
  );
}

export default Summary;