import type { Effect, Reducer } from 'umi';

import { current, query as queryUsers } from '@/services/user';
import { setAuthority, setFunctions, setUserInfo } from '@/utils/authority';

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
      const sysInfo = yield call(current);
      console.log('sysinfo ', sysInfo);
      const response = {
        name: sysInfo.currentUser?.userName,
        userid: sysInfo.currentUser?.userId,
        passwordInited: sysInfo.passwordInited,
        currentAuthority: sysInfo.currentUser?.currentAuthority,
        currentFunctions: sysInfo.currentUser?.currentFunctions
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
