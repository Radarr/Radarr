import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeMoviesModalContent from './OrganizeMoviesModalContent';

interface OrganizeMoviesModalProps {
  isOpen: boolean;
  movieIds: number[];
  onModalClose: () => void;
}

function OrganizeMoviesModal(props: OrganizeMoviesModalProps) {
  const { isOpen, onModalClose, ...otherProps } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <OrganizeMoviesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default OrganizeMoviesModal;
