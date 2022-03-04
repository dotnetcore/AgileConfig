import type { Effect, Reducer } from 'umi';

import { current, query as queryUsers } from '@/services/user';
import { setAuthority, setFunctions, setUserInfo } from '@/utils/authority';
import { sys } from '@/services/system';
import { setSysInfo } from '@/utils/system';

export type CurrentUser = {
  avatar?: string;
  name?: string;
  title?: string;
  group?: string;
  signature?: string;
  tags?: {
    key: string;
    label: string;
  }[];
  userid?: string;
  unreadCount?: number;
};

export type UserModelState = {
  currentUser?: CurrentUser;
};

export type UserModelType = {
  namespace: 'user';
  state: UserModelState;
  effects: {
    fetch: Effect;
    fetchCurrent: Effect;
  };
  reducers: {
    saveCurrentUser: Reducer<UserModelState>;
    changeNotifyCount: Reducer<UserModelState>;
  };
};

const UserModel: UserModelType = {
  namespace: 'user',

  state: {
    currentUser: {},
  },

  effects: {
    *fetch(_, { call, put }) {
      const response = yield call(queryUsers);
      yield put({
        type: 'save',
        payload: response,
      });
    },
    *fetchCurrent(_, { call, put }) {
      const currentInfo = yield call(current);
      console.log('current ', current);
      const sysInfo = yield call(sys);
      console.log('sys', sysInfo);
      const response = {
        name: currentInfo.currentUser?.userName,
        userid: currentInfo.currentUser?.userId,
        passwordInited: sysInfo.passwordInited,
        appVer: sysInfo.appVer,
        envList: sysInfo.envList,
        currentAuthority: currentInfo.currentUser?.currentAuthority,
        currentFunctions: currentInfo.currentUser?.currentFunctions
      };
      yield put({
        type: 'saveCurrentUser',
        payload: response,
      });
    },
  },

  reducers: {
    saveCurrentUser(state, action) {
      setAuthority(action.payload.currentAuthority);
      setFunctions(action.payload.currentFunctions);
      setUserInfo({name:action.payload.name, userid: action.payload.userid});
      setSysInfo(action.payload.appVer, action.payload.envList);
      return {
        ...state,
        currentUser: action.payload || {},
      };
    },
    changeNotifyCount(
      state = {
        currentUser: {},
      },
      action,
    ) {
      return {
        ...state,
        currentUser: {
          ...state.currentUser,
          notifyCount: action.payload.totalCount,
          unreadCount: action.payload.unreadCount,
        },
      };
    },
  },
};

export default UserModel;
