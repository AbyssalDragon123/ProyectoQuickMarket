import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequest } from '../Interface/login-request.interface';
import { LoginResponse } from '../Interface/login-response.interface';
import { RegisterRequest } from '../Interface/register-request.interface';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'https://localhost:7205/api/Auth';
  private TOKEN_KEY = 'qm.jwt';

  constructor(private http: HttpClient) {}

  // TOKEN
  setToken(token: string) { localStorage.setItem(this.TOKEN_KEY, token); }
  getToken(): string | null { return localStorage.getItem(this.TOKEN_KEY); }
  clearToken() { localStorage.removeItem(this.TOKEN_KEY); }

  // API
  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, payload);
  }

  register(payload: RegisterRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, payload);
  }

  me(): Observable<any> {
    const token = this.getToken();
    if (!token) throw new Error('No hay token. Inicia sesión primero.');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.get<any>(`${this.apiUrl}/me`, { headers });
  }
}
