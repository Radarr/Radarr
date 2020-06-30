import PropTypes from 'prop-types';
import React from 'react';
import AuthorImage from 'Author/AuthorImage';

const coverPlaceholder = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPcAAAD3AQMAAAD7QlAQAAAABlBMVEUnJychISEIs8G4AAAEFklEQVRYw+2YMdPOQBDH95KQDEVSMKOj1FFQJx9BQc0nkeuUvoLS+AQ6MQqlRu8xCjpUgsjK7iXz2t1LMsbo8i/ey5vfc3e7m7tk9+DQoUOHDpGe3bu7hS8BwJ117BoAOLfOb/Hf62s4EY1VNrcPVvjNua1WuJ/b8xqoeR3sqFkllx8+AYAra9PniDg1ydr07cT7FQMy6k7ycQMKgJr5F4BrhvI9ZA3xCDU8fJggs9gBXJ35acX8lil74CPmO5w1xhwoIMVFMQcqKCfynH3soLLuEfkB4O5TBArDPZlH05ZkYMxBigyJDEyseylHFjjK4CzPyS4IE3gTgIxuAyulHzbG/as0PYsifM24X8/TA19Vxn2efjagNwFoHE2/GDAKpm86HE2AfMrmLQbqADnI2bzFQPv8y7NlM7naORU+uid+62X4xJg0V6PC1+KfvvSghWMgnh0cVIArCO694Ib+qWR4HQ257F9oRxu+L2FpzK3h7D5vPwqA5k1OPOwA4iaAOYWnZM4XPhPYT3eWDXriX4sHROjpskF7cC2eBHfUdVjeDw6/4Uk9oHqEz18DH9se8IvgCdQDBS/oLUxcPcB24mnAv+jfXvCMOdwI9jNXDxiJp9w9DCd4Afgdz96fF5GGk3xSCFBHw+gF4PAz9SQCwE7K5UGculJHGuTdKPun+IYHrafAUPfPKJdP4OhL7ErDuf9jfnXn6Gu6+Kj654EPKQIG7iu5PMLacGPO7Qf0EOMvx3LhhRh/5l+GOsahnPkw4Mw7sXzLedzxV+DvscsMZ8X51W0Olp/+5P7qIPlLPMEWP+3z5G94rXinuen/RWzAbe6g7hVvRX/DO8FdjMPB9+O3yD5fwf1fc72+/jcfN/cHRPZPJva/7q/27z9zlPyVfL9Abrgv/oW/Nvyx5vL9rbl5f78R/I3iTnP7fRH83QjVDpfCb4Kr71uxz1FzkN9nxfX32XKVHyj+BfweV/mJkM5Pdnkpsc6PfK64BynDM8lTiU1+l+LPP2iLUJj8sj5z3uaXgMPZFDY/rQDHs/rLTRxMfkwx4mX4hPLjaza/TgIfI/l1xvl5y/wT5+dSCd8rmXf8W2/qgx5S5rRYvAMlri+Ic2MKME9FCdQT/wJ8Ga1vSnzE+Z3l06REJi7qI1VfOXw0xusrCPVZ+6aP12dFqO/qN6d4fZeF+rB804X6sInXl/lrT1vBFtAu1KcuCfWpi9e33VLfJjZAS33ckvlZpH4uedu2nOcWhleiPr9peLFT32fyfGD7fMGBlf/jfCLZOd8oIrw6q4/o2jogzlc2z2fAW8w2nwvd3eqp0YXxCcdiS1HzRC8fw2ezJjvHVtn2tPbhqnOzTgNp1/kdv6pV7ig4RQOruuDBCax1+94dOHTo0KFDk34DoJynpPus3GIAAAAASUVORK5CYII=';

function BookCover(props) {
  return (
    <AuthorImage
      {...props}
      coverType="cover"
      placeholder={coverPlaceholder}
    />
  );
}

BookCover.propTypes = {
  size: PropTypes.number.isRequired
};

BookCover.defaultProps = {
  size: 250
};

export default BookCover;
