import { getEnvList } from "@/utils/system";
import { message, Modal, Checkbox } from "antd";
import { CheckboxValueType } from "antd/lib/checkbox/Group";
import React from 'react';
import { useIntl } from "umi";
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
    const intl = useIntl();
    const [checkedList, setCheckedList] = React.useState<CheckboxValueType[]>([]);
    const envList = getEnvList();
    const onChange = (list:CheckboxValueType[]) => {
      setCheckedList(list);
    };

    return (
        <Modal 
          title={intl.formatMessage({id: 'pages.configs.sync_env_title'})}
          visible={props.ModalVisible}
          onCancel={
            ()=>{
              props.onCancel();
            }
          }
          onOk={
            async ()=> {
              if (!checkedList.length) {
                message.error(intl.formatMessage({id: 'pages.configs.select_at_least_one_env'}));
                return;
              }
              const hide = message.loading(intl.formatMessage({id: 'pages.configs.syncing'}));
              try {
                const result = await envSync(props.appId, props.currentEnv, checkedList.map(item=>item.toString()));
                const success = result.success;
                if (success) {
                  props.onSaveSuccess();
                  message.success(intl.formatMessage({id: 'pages.configs.sync_success'}));
                } else {
                  message.error(intl.formatMessage({id: 'pages.configs.sync_failed'}));
                }
              }
              catch (e) {
                message.error(intl.formatMessage({id: 'pages.configs.sync_failed'}));
              }
              finally {
                hide();
              }
            }
          }
          >
          {intl.formatMessage({id: 'pages.configs.sync_from_to'}, {env: props.currentEnv})}
          <div style={{marginTop:20}}>
            <CheckboxGroup options={envList.filter(x=> x !== props.currentEnv)} value={checkedList} onChange={onChange}  />
          </div>
        </Modal>
    );
}

export default EnvSync;