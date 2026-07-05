import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppNotification } from '../models/notification.model';

import { API_BASE_URL } from '../api-config';
const API_URL = `${API_BASE_URL}/notifications`;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private http: HttpClient) {}

  getMyNotifications(): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(API_URL);
  }

  markAsRead(id: number): Observable<void> {
    return this.http.put<void>(`${API_URL}/${id}/read`, {});
  }
}
