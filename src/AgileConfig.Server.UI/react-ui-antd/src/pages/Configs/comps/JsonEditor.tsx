import { Button, message, Modal, Checkbox, Tooltip } from 'antd';
import React, { useState, useEffect } from 'react';
import { getConfigJson, saveJson } from '../service';
import Editor from '@monaco-editor/react';
import { loader } from '@monaco-editor/react';
import { CheckboxChangeEvent } from 'antd/lib/checkbox/Checkbox';
import { QuestionCircleOutlined } from '@ant-design/icons';
import { useIntl } from 'umi';
loader.config({ paths: { vs: 'monaco-editor/min/vs' } });

export type JsonEditorProps = {
  appId: string;
  appName: string;
  ModalVisible: boolean;
  env: string;
  onCancel: () => void;
  onSaveSuccess: () => void;
};

const JsonEditor: React.FC<JsonEditorProps> = (props) => {
  const intl = useIntl();
  const [json, setJson] = useState<string>();
  const [jsonValidateSuccess, setJsonValidateSuccess] = useState<boolean>(true);
  const [isPatch, setIsPatch] = useState<boolean>(false);

  const handleSave = async (json: string, appId: string, env: string, isPatch: boolean) => {
    const hide = message.loading(intl.formatMessage({
      id: 'saving'
    }));
    try {
      const result = await saveJson(appId, env, json, isPatch);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({
          id: 'save_success'
        }));
      } else {
        message.error(intl.formatMessage({
          id: 'save_fail'
        }));
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({
        id: 'save_fail'
      }));
      return false;
    }
  };

  const onIsPatchChange = (e: CheckboxChangeEvent) => {
    setIsPatch(e.target.checked);
  };

  useEffect(() => {
    getConfigJson(props.appId, props.env).then((res) => {
      if (res.success) {
        let jsonObj = JSON.parse(res.data);
        console.log(jsonObj);
        setJson(res.data);
        console.log(res.data);
        console.log(json);
      }
    });
  }, []);
  return (
    <Modal
      maskClosable={false}
      title={intl.formatMessage({id: 'pages.configs.json_editor_title'})}
      okText={intl.formatMessage({id: 'pages.configs.save'})}
      width={800}
      visible={props.ModalVisible}
      onCancel={props.onCancel}
      footer={
        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
          <div>
            {jsonValidateSuccess ? <></> : <span style={{ color: 'red' }}>{intl.formatMessage({id: 'pages.configs.json_invalid'})}</span>}
          </div>
          <div>
            <span style={{ marginRight: '12px' }}>
              <Checkbox onChange={onIsPatchChange} value={isPatch}>
                {intl.formatMessage({id: 'pages.configs.patch_mode'})}
              </Checkbox>
              <Tooltip title={intl.formatMessage({id: 'pages.configs.patch_mode_tooltip'})}>
                <QuestionCircleOutlined />
              </Tooltip>
            </span>

            <Button
              onClick={() => {
                props.onCancel();
              }}
            >
              {intl.formatMessage({id: 'pages.configs.cancel'})}
            </Button>
            <Button
              type="primary"
              onClick={async () => {
                if (json && jsonValidateSuccess) {
                  const saveResult = await handleSave(json, props.appId, props.env, isPatch);
                  if (saveResult) {
                    props.onSaveSuccess();
                  }
                } else {
                  message.error(intl.formatMessage({id: 'pages.configs.json_invalid'}));
                }
              }}
            >
              {intl.formatMessage({id: 'pages.configs.save'})}
            </Button>
          </div>
        </div>
      }
    >
      <Editor
        height="500px"
        defaultLanguage="json"
        defaultValue=""
        value={json}
        options={{ minimap: { enabled: false } }}
        beforeMount={(monaco) => {
          monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
            validate: true,
            allowComments: true,//是否允许json内容中带注释
            schemaValidation: 'error',
          });
        }}
        onChange={(v, e) => {
          setJson(v);
        }}
        onValidate={(markers) => {
          setJsonValidateSuccess(markers.length == 0);
        }}
      />
    </Modal>
  );
};

export default JsonEditor;
