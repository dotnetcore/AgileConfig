import { AppstoreOutlined, DatabaseOutlined, ShrinkOutlined, TableOutlined } from "@ant-design/icons";
import React, { useEffect, useState } from "react";
import { queryAppcount, queryConfigcount, queryNodecount } from "../service";
import ItemInfo from "./itemInfo";
import styles from '../index.less';

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

  export default Summary;