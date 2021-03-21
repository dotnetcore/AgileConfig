import { getToken } from "@/utils/authority";
import { DeleteOutlined, UploadOutlined } from "@ant-design/icons";
import { Button, message, Modal, Space, Table, Upload } from "antd";
import React, { useState } from 'react';
import { JsonImportItem } from "../data";
import { addRangeConfig } from "../service";
import styles from './jsonImport.less';
export type JsonImportFormProps = {
    appId: string,
    appName: string,
    jsonImportModalVisible: boolean;
    onCancel: () => void;
    onSaveSuccess: ()=> void;
  };
  const handleSave = async ( items: JsonImportItem[]) => {
    const hide = message.loading('正在导入');
    try {
      const result = await addRangeConfig(items);
      hide();
      const success = result.success;
      if (success) {
        message.success('导入成功');
      } else {
        message.error(result.message);
      }
      return success;
    } catch (error) {
      hide();
      message.error('导入失败请重试！');
      return false;
    }
  };
const JsonImport : React.FC<JsonImportFormProps> = (props)=>{
    const [datasource, setDatasource] = useState<JsonImportItem[]>([]);
    const deleteItem = (item:JsonImportItem) => {
      const index = datasource.findIndex(x=>x.id === item.id);
      console.log(index, item.id);
      if (index >= 0) {
        datasource.splice(index, 1);
        const cloned  = Object.assign([], datasource)
        setDatasource(cloned);
      }
    }
    const columns = [
        {
          title: '组',
          dataIndex: 'group',
        },
        {
          title: '键',
          dataIndex: 'key',
        },
        {
          title: '值',
          dataIndex: 'value',
        },
        {
          title: '操作',
          key: 'action',
          render: (text:string, record:any) => (
            <Space size="middle">
              <a title="删除" onClick={ ()=>{ deleteItem(record) } }>
                <DeleteOutlined />
              </a>
            </Space>
          ),
        }
      ];
      const fileUploadProps = {
        name: 'file',
        action: 'http://localhost:5000/config/PreViewJsonFile',
        headers: {
          Authorization:  'Bearer ' + getToken(),
        },
        onChange(info:any) {
          if (info.file.status !== 'uploading') {
            console.log(info.file, info.fileList);
          }
          if (info.file.status === 'done') {
            console.log('uplaod done');
            if(info.file.percent === 100) {
              const response = info.file.response;
              if (response.success) {
                const itemList = response.data;
                itemList.forEach((x:any)=>{
                  x.appId = props.appId;
                });
                setDatasource(itemList);
              }
            }
          } else if (info.file.status === 'error') {
            message.error(`${info.file.name} 上传失败.`);
          }
        },
      };
    return (
        <Modal 
          title="从json文件导入" 
          width={1000} 
          visible={props.jsonImportModalVisible}
          onCancel={
            ()=>{
              props.onCancel();
            }
          }
          onOk={
            async ()=> {
              const result = await handleSave(datasource);
              if (result) {
                props.onSaveSuccess();
              }
            }
          }
          >
            <div className={styles.action_bar}>
              <Upload accept=".json" {...fileUploadProps}>
                <Button type="primary" icon={<UploadOutlined />}>选择文件</Button>
              </Upload>
            </div>
            <Table 
              rowKey="id"
              pagination={false}
              dataSource={datasource} 
              columns={columns}
              scroll ={{
                y: 500
              }}>
              
            </Table>
        </Modal>
    );
}

export default JsonImport;