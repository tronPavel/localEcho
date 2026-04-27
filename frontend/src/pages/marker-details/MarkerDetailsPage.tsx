import {RoutedModal} from "@/shared/ui/Modal/RoutedModal.tsx";
import {useParams} from "react-router-dom";
import {MarkerDetailsWidget} from "@/widgets/MarkerDetailsPanel/ui/MarkerDetailsWidget.tsx";

export const MarkerDetailsPage = () => {
    const { id } = useParams();
    return (
        <RoutedModal>
            {id && <MarkerDetailsWidget id={id} />}
        </RoutedModal>
    );
}