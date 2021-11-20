import { Button, message, Modal, Input } from "antd";
const { TextArea } = Input;
import React, { useState, useEffect } from 'react';
import { getConfigJson, saveJson } from "../service";
import Editor from "@monaco-editor/react";

const handleSave = async (json: string, appId:string, env:string) => {
  const hide = message.loading('正在保存...');
  try {
    const result = await saveJson(appId, env, json);
    hide();
    const success = result.success;
    if (success) {
      message.success('保存成功！');
    } else {
      message.error('保存失败！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('保存失败！');
    return false;
  }
};

export type JsonEditorProps = {
    appId: string,
    appName: string,
    ModalVisible: boolean;
    env: string,
    onCancel: () => void;
    onSaveSuccess: ()=> void;
  };

const JsonEditor : React.FC<JsonEditorProps> = (props)=>{
    const [json, setJson] = useState<string>();
    useEffect(()=>{
      getConfigJson(props.appId, props.env).then(res=>{
        if (res.success) {
          let jsonObj = JSON.parse(res.data);
          console.log(jsonObj);
          setJson(res.data);
          console.log(res.data);
          console.log(json);
        }
      });
    },[])
    return (
        <Modal 
          title="按 JSON 视图编辑"
          okText="保存"
          width={800} 
          visible={props.ModalVisible}
          onCancel={
            ()=>{
              props.onCancel();
            }
          }
          onOk={
            async ()=> {
              if(json) {
                const saveResult = await handleSave(json, props.appId, props.env);
                if (saveResult) {
                  props.onSaveSuccess();
                }
              }else{
                message.error('json 文本不能为空。');
              }
            }
          }
          >
            <Editor
              height="500px"
              defaultLanguage="json"
              defaultValue=''
              value={json}
              options= { {minimap: { enabled:false } }}
              onChange= { (v, e)=> {
                setJson(v);
              }}
            />
        </Modal>
    );
}

export default JsonEditor;