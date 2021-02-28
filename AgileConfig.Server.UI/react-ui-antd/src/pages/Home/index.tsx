import { AppstoreOutlined, DatabaseOutlined, HomeFilled, ShrinkOutlined, TableOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import React from 'react';
import styles from './index.less';

export type itemInfoProps = {
  type:string,
  icon: JSX.Element,
  count: number
}

const ItemInfo: React.FC<itemInfoProps> = (props) => {
  const itemColor=()=>{
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
  const itemName=()=>{
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
    <div  className={styles.item} style={{backgroundColor:itemColor()}}>
      <div>
        <div>{props.count}</div>
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
  return (
    <div className={styles.summary}>
       <ItemInfo count={10} type="node" icon={<DatabaseOutlined ></DatabaseOutlined>}></ItemInfo>
       <ItemInfo count={20} type="app" icon={<AppstoreOutlined ></AppstoreOutlined>}></ItemInfo>
       <ItemInfo count={30} type="config" icon={<TableOutlined ></TableOutlined>}></ItemInfo>
       <ItemInfo count={40} type="client" icon={<ShrinkOutlined ></ShrinkOutlined>}></ItemInfo>
       
    </div>
  );
}
const home:React.FC = () =>  {
  return (
    <PageContainer pageHeaderRender={false}>
     <Summary></Summary>  
    </PageContainer>
  );
}
export default home;
