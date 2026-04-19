import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { Modal } from './Modal';
import {ModalHeader} from "@/shared/ui/Modal/ModalHeader.tsx";

interface RoutedModalProps {
    children: ReactNode;
    title?: string;
}

export const RoutedModal = ({ children, title}: RoutedModalProps) => {
    const navigate = useNavigate();
    return (
        <Modal isOpen={true} onClose={() => navigate('/')}>
            {title && <ModalHeader title={title} />}
            <div>
                {children}
            </div>
        </Modal>
    );
};