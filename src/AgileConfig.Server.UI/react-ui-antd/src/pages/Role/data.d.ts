export type RoleItem = {
  id: string;
  name: string;
  description?: string;
  isSystem: boolean;
  functions: string[];
};

export type RoleFormValues = {
  id?: string;
  name: string;
  description?: string;
  functions: string[];
  isSystem?: boolean;
};
