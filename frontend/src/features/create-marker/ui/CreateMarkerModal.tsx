import { Modal } from '../../../shared/ui/Modal/Modal';
import { CreateMarkerForm } from './CreateMarkerForm';
import { useCreateMarkerStore } from '../model/createMarkerStore';

export const CreateMarkerModal = () => {
    const { isModalOpen, closeModal } = useCreateMarkerStore();

    return (
        <Modal isOpen={isModalOpen} onClose={closeModal}>
            <CreateMarkerForm />
        </Modal>
    );
};