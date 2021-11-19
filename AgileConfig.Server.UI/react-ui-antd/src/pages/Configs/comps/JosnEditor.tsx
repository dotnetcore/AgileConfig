import { Button, message, Modal, Input } from "antd";
const { TextArea } = Input;
import React, { useState, useEffect } from 'react';
import { getConfigJson } from "../service";
import Editor from "@monaco-editor/react";

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
          title="按 json 视图编辑"
          okText="保存"
          width={1000} 
          visible={props.ModalVisible}
          onCancel={
            ()=>{
              props.onCancel();
            }
          }
          onOk={
            async ()=> {
              props.onSaveSuccess();
            }
          }
          >
            <Editor
              height="400px"
              defaultLanguage="json"
              defaultValue=''
              value={json}
            />
        </Modal>
    );
}

export default JsonEditor;