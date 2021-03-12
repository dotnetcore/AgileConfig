import { getToken } from "@/utils/authority";
import { UploadOutlined } from "@ant-design/icons";
import { Button, message, Modal, Table, Upload } from "antd";
import { List } from "lodash";
import React, { useState } from 'react';
import styles from './jsonImport.less';
export type JsonImportFormProps = {
    appId: string,
    appName: string,
    jsonImportModalVisible: boolean;
    onCancel: () => void;
  };

const JsonImport : React.FC<JsonImportFormProps> = (props)=>{
    const [datasource, setDatasource] = useState<{group:string,key:string, value:string}[]>([]);
    const columns = [
        {
          title: '组',
          dataIndex: 'group',
          key: 'group',
        },
        {
          title: '键',
          dataIndex: 'key',
          key: 'key',
        },
        {
          title: '值',
          dataIndex: 'value',
          key: 'value',
        },
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
          >
            <div className={styles.action_bar}>
              <Upload accept=".json" {...fileUploadProps}>
                <Button type="primary" icon={<UploadOutlined />}>选择文件</Button>
              </Upload>
            </div>
            <Table dataSource={datasource} columns={columns}>
            </Table>
        </Modal>
    );
}

export default JsonImport;