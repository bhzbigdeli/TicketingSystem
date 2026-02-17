import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.jsx';
import './styles.css';

const rootElement = document.getElementById('root');

if (!rootElement) {
  document.body.innerHTML = '<h2 style="font-family:sans-serif;padding:16px">React root element not found.</h2>';
} else {
  ReactDOM.createRoot(rootElement).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
}
