import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './app/App';
import { QueryProvider } from './app/providers';
import 'leaflet/dist/leaflet.css';
import './index.css';

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <QueryProvider>
            <App />
        </QueryProvider>
    </StrictMode>,
);