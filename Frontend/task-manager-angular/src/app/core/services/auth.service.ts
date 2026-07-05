import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/user.model';

import { API_BASE_URL } from '../api-config';
const API_URL = `${API_BASE_URL}/auth`;
const TOKEN_KEY = 'tms_token';
const USER_KEY = 'tms_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  currentUser = signal<AuthResponse | null>(this.loadStoredUser());

  constructor(private http: HttpClient) {}

  private loadStoredUser(): AuthResponse | null {
    const raw = localStorage.getItem(USER_KEY) || sessionStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_URL}/register`, payload).pipe(
      tap(res => this.persistSession(res, true))
    );
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_URL}/login`, payload).pipe(
      tap(res => this.persistSession(res, payload.rememberMe))
    );
  }

  logout(): void {
    this.http.post(`${API_URL}/logout`, {}).subscribe({ complete: () => this.clearSession() });
  }

  private persistSession(res: AuthResponse, rememberMe: boolean): void {
    const store = rememberMe ? localStorage : sessionStorage;
    store.setItem(TOKEN_KEY, res.token);
    store.setItem(USER_KEY, JSON.stringify(res));
    this.currentUser.set(res);
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this.currentUser.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.currentUser()?.role === 'Admin';
  }
}
