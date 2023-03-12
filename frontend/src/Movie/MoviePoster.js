import PropTypes from 'prop-types';
import React from 'react';
import MovieImage from './MovieImage';

const posterPlaceholder = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKgAAAD3CAMAAAC+Te+kAAAAZlBMVEUvLi8vLy8vLzAvMDAwLy8wLzAwMDAwMDEwMTExMDAxMDExMTExMTIxMjIyMjIyMjMyMzMzMjMzMzMzMzQzNDQ0NDQ0NDU0NTU1NTU1NTY1NjY2NTY2NjY2Njc2Nzc3Njc3Nzc3NziHChLWAAAAAWJLR0QAiAUdSAAAAAlwSFlzAAALEwAACxMBAJqcGAAAAAd0SU1FB+MKCgEdHeShUbsAAALZSURBVHja7dxNcuwgDEZR1qAVmP1vMrNUJe91GfTzCSpXo575lAymjYWGXRIDKFCgQIECBQoUKFCgQIECBQoUKFCgQIECBQoUKFCgQIECBQoUKNA/AZ3fcTR0/owjofNDnAadnwPoPnS+xTXQeQZ0rkQ/dC4H0Gzo7ITO3bgGOnug/2PcAF3Mczt0fUj0QncG7znQBupw3PkWqh8qpkagpnyqjuArkkxaC02kRqGypCZANVYFdJZCdy9WTRVB5znQ6qTmjFFBWnOhdg20Lqnp0CpqAbRmAJRAK5JaA32zngTNvv910OSkVkJTs1oLtWugeTkNQZ/nkT2rotBHldUwNE6VQTVWGTQ6AHKggqGaBS23JkKf0hUgE1qa01Ro5fzPhoapR0HtCGg4q0poSCqFRgaAFhqxqqEr1EOgmdJaqHdaHQq1I6CunPZAHdY2aIJUBN2V9kE3H1Wd0BXrNVA7BLpgdUCtALo8pZqhdgd0Z6OyE7q1pdoH3dv7tS7o7iZ1E3R/N70Huuz795cQao65vvkqooT+vEgDdPcbj2s3zxTv9Qt/7cuhdgfUo2yAOplyqNuphfqZSqhFmEJo0HkcdPZCo0rRymRxpwSawHR+YtyBZihfvi+nQO0OqCmcYahGqYPGS4qCUJkzBpUpJdCkordyaFZxXi1UUpaZAJ2XQFOLh8ug2XXjVdD0+vYiqLIO3w1VH8EogtoxUPnpGxe04zyTA1p57i4T2nTmbnnnUuLMg1afYE2C1h+1zYEKjlknQLtPg9tb3YzU+dL054qOBb8cvcz3DlqBZhUmhdrnKo9j+pR0rkN5UHkznZHPtJIYN2TTCe1poTUyk9nWPO0bt8Ys7Ug34mlUMONtPUXMaEdXnXN1MnUzN2Z9q3Lr8XQN1DaLQJpXpiamZwltYdIUHShQoECBAgUKFChQoECBAgUKFChQoECBAgUKFChQoECBAgUKFCjQ+vgCff/mEp/vtiIAAAAASUVORK5CYII=';

function MoviePoster(props) {
  return (
    <MovieImage
      {...props}
      coverType="poster"
      placeholder={posterPlaceholder}
    />
  );
}

MoviePoster.propTypes = {
  ...MovieImage.propTypes,
  coverType: PropTypes.string,
  placeholder: PropTypes.string,
  overflow: PropTypes.bool,
  size: PropTypes.number.isRequired
};

MoviePoster.defaultProps = {
  size: 250
};

export default MoviePoster;
