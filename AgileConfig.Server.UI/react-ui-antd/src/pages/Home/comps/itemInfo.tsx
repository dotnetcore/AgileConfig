import styles from '../index.less';
import { history } from 'umi';

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

  export default ItemInfo;