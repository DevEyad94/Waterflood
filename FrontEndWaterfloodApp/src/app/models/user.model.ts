export interface User {
  userID?: number;
  username: string;
  fullName: string;
  phoneNumber: string;
  role: string[];
  token: string;
}

export interface UserLoginDto {
  username: string;
  passwordHash: string;
}

export interface UserResponse {
  data: User;
  success: boolean;
  message: string;
}

export interface UserRegisterDto {
  username: string;
  passwordHash: string;
  fullName: string;
  zRoleId: number;
}

export interface UserUpdateDto {
  userID: number;
  fullName: string;
  passwordHash?: string;
  zRoleId?: number;
}

export interface RoleListItem {
  zRoleId: number;
  name: string;
}
