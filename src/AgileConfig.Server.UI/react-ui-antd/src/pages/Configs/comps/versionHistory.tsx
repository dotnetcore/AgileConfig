import { Button, Col, message, Modal, Row, Space, Table, Tag } from "antd";
import styles from './versionHistory.less';
import moment from "moment";
import React, { useState,useEffect } from 'react';
import { useIntl } from "react-intl";
import { PublishDetialNode } from "../data";
import { getPublishHistory, rollback } from "../service";
import { ExclamationCircleOutlined } from "@ant-design/icons";

const handleRollback = async (timelineId: string, env: string) => {
  const hide = message.loading('正在回滚');
  try {
    const result = await rollback(timelineId, env);
    hide();
    const success = result.success;
    if (success) {
      message.success('回滚成功！');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('回滚失败！');
    return false;
  }
};

export type VersionHistoryFormProps = {
    appId: string,
    appName: string,
    versionHistoryModalVisible: boolean;
    env: string,
    onCancel: (reload: boolean) => void;
    onSaveSuccess: ()=> void;
  };
const { confirm } = Modal;
const VersionHistory : React.FC<VersionHistoryFormProps> = (props)=>{
  const intl = useIntl();
  const editStatusEnums = {
    0: '新增',
    1: '编辑',
    2: '删除',
    10: '已提交'
  }
  const editStatusColors = {
    0: 'blue',
    1: 'gold',
    2: 'red',
    10: ''
  }
    const [datasource, setDatasource] = useState<PublishDetialNode[]>([]);
    useEffect(() => {
      getPublishHistory(props.appId, props.env).then(resp => {
        if (resp.success) {
          setDatasource(resp.data);
        }   
      })
    }, []);
    const columns = [
        {
          width: 200,
          title: intl.formatMessage({id:'pages.configs.table.cols.g'}),
          dataIndex: 'group',
          ellipsis: true
        },
        {
          width: 200,
          title: intl.formatMessage({id:'pages.configs.table.cols.k'}),
          dataIndex: 'key',
          ellipsis: true
        },
        {
          title: intl.formatMessage({id:'pages.configs.table.cols.v'}),
          dataIndex: 'value',
          ellipsis: true
        },
        {
            width: 150,
            title: '编辑状态',
            dataIndex: 'editStatus',
            render: (_:any, record:any) => (
              <Tag color={editStatusColors[record.editStatus]}>
                {
                   editStatusEnums[record.editStatus]
                }
              </Tag>
           ),
          },
      ];
    return (
        <Modal 
          footer={false}
          cancelText="关闭"
          title="历史版本"
          width={1000} 
          visible={props.versionHistoryModalVisible}
          onCancel={
            ()=>{
              props.onCancel(false);
            }
          }
          >
            <div className={styles.historyContainer}>
            {
              datasource.length === 0 ?
              '暂无数据'
              :
              datasource.map( (e, i)=> 
                <div key={e.key} className={styles.historyVersionTable}>
                  <Table 
                    key={e.key}
                    rowKey="id"
                    size="small"
                    title={
                      (row) => {
                        return <>
                        {
                          moment(e.timelineNode.publishTime).format('YYYY-MM-DD HH:mm:ss')+' / ' +e.timelineNode.publishUserName + '  '
                        }
                        {
                          i === 0 ? <Tag>当前版本</Tag> : ''
                        }
                        <div style={{color:"#8c8c8c"}}>
                          {
                            e.timelineNode.log
                          }
                        </div>
                        </>
                      }
                    }
                    pagination={false}
                    dataSource={e.list} 
                    columns={columns}
                    >
                  </Table>
                  <div>
                  <Row justify="end">
                    <Col span={2} >
                      <Button type="primary" style={{marginTop:20}} hidden={i===0} 
                        onClick={()=>{
                          confirm({
                            onOk:async ()=>{
                              const result = await handleRollback(e.timelineNode.id, props.env);
                              if (result) {
                                props.onCancel(true)
                              }  
                            },
                            icon: <ExclamationCircleOutlined />,
                            content: <div>
                              {`确定回滚至【${moment(e.timelineNode.publishTime).format('YYYY-MM-DD HH:mm:ss')}】时刻的发布版本吗？`}
                              <br></br>
                              <br></br>
                              <div>
                                注意：本操作会清空当前所有待发布的配置项
                              </div>
                            </div>
                          });
                        }}
                      >
                        回滚
                      </Button >
                    </Col>
                  </Row>
                   
                  </div>
                </div>
                
              )
            }
            </div>
        </Modal>
    );
}

export default VersionHistory;