import type { Reducer } from '@umijs/max';
import type { DefaultSettings } from '../../config/defaultSettings';
import defaultSettings from '../../config/defaultSettings';
import { applyDarkMode, getStoredDarkMode } from '@/utils/theme';

export type SettingModelType = {
  namespace: 'settings';
  state: DefaultSettings;
  reducers: {
    changeSetting: Reducer<DefaultSettings>;
  };
};

const updateColorWeak: (colorWeak: boolean) => void = (colorWeak) => {
  const root = document.getElementById('root');
  if (root) {
    root.className = colorWeak ? 'colorWeak' : '';
  }
};

const initialSettings: DefaultSettings = {
  ...defaultSettings,
  darkMode: getStoredDarkMode(),
};

applyDarkMode(initialSettings.darkMode);

const SettingModel: SettingModelType = {
  namespace: 'settings',
  state: initialSettings,
  reducers: {
    changeSetting(state = initialSettings, { payload }) {
      const { colorWeak, contentWidth, darkMode } = payload;

      if (state.contentWidth !== contentWidth && window.dispatchEvent) {
        window.dispatchEvent(new Event('resize'));
      }
      updateColorWeak(!!colorWeak);
      if (typeof darkMode === 'boolean') {
        applyDarkMode(darkMode);
      }
      return {
        ...state,
        ...payload,
      };
    },
  },
};
export default SettingModel;
