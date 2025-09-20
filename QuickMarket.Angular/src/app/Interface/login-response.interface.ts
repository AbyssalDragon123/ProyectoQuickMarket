export interface LoginResponse {
  message: string;
  token: string;
  user: {
    id_LOGIN: number;
    username: string;
    gmail: string;
    id_EMPLEADO: number;
  };
}
