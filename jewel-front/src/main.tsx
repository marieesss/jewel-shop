import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import { rootStore } from './stores/context';
import './index.css';

void rootStore.auth.restoreSession();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
