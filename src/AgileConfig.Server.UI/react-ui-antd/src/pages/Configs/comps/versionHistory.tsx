import { Button, Col, message, Modal, Row, Table, Tag } from "antd";
import styles from './versionHistory.less';
import moment from "moment";
import React, { useState,useEffect } from 'react';
import { useIntl } from "umi";
import { PublishDetialNode } from "../data";
import { getPublishHistory, rollback } from "../service";
import { ExclamationCircleOutlined } from "@ant-design/icons";

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

  const handleRollback = async (timelineId: string, env: string) => {
    const hide = message.loading(intl.formatMessage({
      id: 'pages.configs.version.rolling_back'
    }));
    try {
      const result = await rollback(timelineId, env);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({
          id: 'pages.configs.version.rollback_success'
        }));
      } else {
        message.error(result.message);
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({
        id: 'pages.configs.version.rollback_fail'
      }));
      return false;
    }
  };

  const editStatusEnums = {
    0: intl.formatMessage({
      id: 'pages.configs.version.status.add'
    }),
    1: intl.formatMessage({
      id: 'pages.configs.version.status.edit'
    }),
    2: intl.formatMessage({
      id: 'pages.configs.version.status.delete'
    }),
    10: intl.formatMessage({
      id: 'pages.configs.version.status.committed'
    })
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
            title: intl.formatMessage({
              id: 'pages.configs.version.editStatus'
            }),
            dataIndex: 'editStatus',
            render: (_:any, record:any) => (
              <Tag color={editStatusColors[record.editStatus as keyof typeof editStatusColors]}>
                {
                   editStatusEnums[record.editStatus as keyof typeof editStatusEnums]
                }
              </Tag>
           ),
          },
      ];
    return (
        <Modal 
          footer={false}
          cancelText={intl.formatMessage({id: 'pages.configs.close'})}
          title={intl.formatMessage({id: 'pages.config.history.title'})}
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
              intl.formatMessage({id: 'pages.configs.no_data'})
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
                          i === 0 ? <Tag>{intl.formatMessage({id: 'pages.config.history.current'})}</Tag> : ''
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
                              {`${intl.formatMessage({id: 'pages.configs.confirm_rollback_to'})}【${moment(e.timelineNode.publishTime).format('YYYY-MM-DD HH:mm:ss')}】${intl.formatMessage({id: 'pages.configs.confirm_rollback_suffix'})}`}
                              <br></br>
                              <br></br>
                              <div>
                                {intl.formatMessage({id: 'pages.configs.rollback_warning'})}
                              </div>
                            </div>
                          });
                        }}
                      >
                        {intl.formatMessage({id: 'pages.config.history.rollback'})}
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