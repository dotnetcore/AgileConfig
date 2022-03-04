import { getEnvList } from "@/utils/system";
import { Button, message, Modal, Space, Table, Upload, Checkbox } from "antd";
import { CheckboxValueType } from "antd/lib/checkbox/Group";
import React, { useState } from 'react';
import { envSync } from "../service";
const CheckboxGroup = Checkbox.Group;
export type EnvSyncFormProps = {
    appId: string,
    currentEnv: string,
    ModalVisible: boolean;
    onCancel: () => void;
    onSaveSuccess: ()=> void;
  };
  
const EnvSync : React.FC<EnvSyncFormProps> = (props)=>{
    const [checkedList, setCheckedList] = React.useState<CheckboxValueType[]>([]);
    const envList = getEnvList();
    const onChange = (list:CheckboxValueType[]) => {
      setCheckedList(list);
    };

    return (
        <Modal 
          title="同步环境"
          visible={props.ModalVisible}
          onCancel={
            ()=>{
              props.onCancel();
            }
          }
          onOk={
            async ()=> {
              if (!checkedList.length) {
                message.error('请至少勾选一个环境');
                return;
              }
              const hide = message.loading('正在同步');
              try {
                const result = await envSync(props.appId, props.currentEnv, checkedList.map(item=>item.toString()));
                const success = result.success;
                if (success) {
                  props.onSaveSuccess();
                  message.success('同步成功！');
                } else {
                  message.error('同步失败');
                }
              }
              catch (e) {
                message.error('同步失败');
              }
              finally {
                hide();
              }
            }
          }
          >
          将当前 {props.currentEnv} 环境的配置同步到：
          <div style={{marginTop:20}}>
            <CheckboxGroup options={envList.filter(x=> x !== props.currentEnv)} value={checkedList} onChange={onChange}  />
          </div>
        </Modal>
    );
}

export default EnvSync;