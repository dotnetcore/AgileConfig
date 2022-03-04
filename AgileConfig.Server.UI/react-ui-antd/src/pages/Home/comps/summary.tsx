import { AppstoreOutlined, DatabaseOutlined, ShrinkOutlined, TableOutlined } from "@ant-design/icons";
import React, { useEffect, useState } from "react";
import { queryAppcount, queryConfigcount } from "../service";
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
  
    useEffect(() => {
      queryConfigcount().then(x => setConfigCount(x));
    }, []);
    useEffect(() => {
      queryAppcount().then(x => setAppCount(x));
    }, []);
  
    return (
      <div className={styles.panel}>
   <Row gutter={16} className={styles.summary}>
          <Col  span={6}>
        <ItemInfo count={props.nodeOnCount + '/' + props.nodeCount} type="node" link="/node" icon={<DatabaseOutlined ></DatabaseOutlined>}></ItemInfo>
          </Col>
          <Col span={6}>
        <ItemInfo count={appCount + ''} type="app" link="/app" icon={<AppstoreOutlined ></AppstoreOutlined>}></ItemInfo>
          </Col>
          <Col span={6}>
        <ItemInfo count={configCount + ''} type="config" link="/app" icon={<TableOutlined ></TableOutlined>}></ItemInfo>
          </Col>
          <Col span={6}>
        <ItemInfo count={props.clientCount + ''} type="client" link="/client" icon={<ShrinkOutlined ></ShrinkOutlined>}></ItemInfo>
          </Col>
      </Row>
      </div>
    );
  }

  export default Summary;