import { message, Modal, Table, Tag } from "antd";
import Title from "antd/lib/skeleton/Title";
import moment from "moment";
import React, { useState,useEffect } from 'react';
import { useIntl } from "react-intl";
import { PublishDetial, PublishDetialNode } from "../data";
import { getPublishHistory } from "../service";
export type VersionHistoryFormProps = {
    appId: string,
    appName: string,
    versionHistoryModalVisible: boolean;
    onCancel: () => void;
    onSaveSuccess: ()=> void;
  };
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
      getPublishHistory(props.appId).then(resp => {
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
              props.onCancel();
            }
          }
          >
            <div style={{
              maxHeight:600,
              overflowY:"scroll"
            }}>
            {
              datasource.map(e=> 
                <div style={{marginBottom: 30}}>
                  <Table 
                    
                    key={e.key}
                    rowKey="id"
                    size="small"
                    title={
                      (row) => {
                        return moment(e.timelineNode.publishTime).format('YYYY-MM-DD HH:mm:ss')+' / ' +e.timelineNode.publishUserId
                      }
                    }
                    pagination={false}
                    dataSource={e.list} 
                    columns={columns}
                    bordered
                    >
                  </Table>
                </div>
                
              )
            }
            </div>
        </Modal>
    );
}

export default VersionHistory;