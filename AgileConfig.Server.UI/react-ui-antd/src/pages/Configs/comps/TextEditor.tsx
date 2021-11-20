import { Button, message, Modal, Input } from "antd";
const { TextArea } = Input;
import Editor from "@monaco-editor/react";
import React, { useState, useEffect } from 'react';
import { getConfigsKvList } from "../service";
export type TextEditorProps = {
    appId: string,
    appName: string,
    ModalVisible: boolean;
    env: string,
    onCancel: () => void;
    onSaveSuccess: ()=> void;
  };

const TextEditor : React.FC<TextEditorProps> = (props)=>{

  const [kvText, setkvText] = useState<string>("");

    useEffect(()=>{
      getConfigsKvList(props.appId, props.env).then(res=>{
        if (res.success) {
          const list = res.data.map((x: { value: string; key: string; }) =>  x.key + ' = ' + x.value);
          setkvText(list.join('\n'));
        }
      });
    },[])

    return (
        <Modal 
          title="按 TEXT 视图编辑"
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
              height="600px"
              defaultLanguage="text"
              defaultValue=''
              value={kvText}
              options= { {minimap: { enabled:false } }}
            />
        </Modal>
    );
}

export default TextEditor;