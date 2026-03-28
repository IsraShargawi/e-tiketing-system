import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

function App() {
  const [tickets, setTickets] = useState([]);
  const [cart, setCart] = useState([]);
  const [paymentMethod, setPaymentMethod] = useState('CreditCard');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchTickets();
  }, []);

  const fetchTickets = async () => {
    try {
      const response = await axios.get(`${API_URL}/tickets`);
      setTickets(response.data);
    } catch (err) {
      setError('Failed to load tickets');
    }
  };

  const getTicketTypeString = (typeValue) => {
    const typeMap = {
      1: 'Gold',
      2: 'Premium',
      3: 'VIP'
    };
    return typeMap[typeValue] || 'Unknown';
  };

  const addToCart = (ticket) => {
    const existing = cart.find(item => item.ticketId === ticket.id);
    if (existing) {
      setCart(cart.map(item =>
        item.ticketId === ticket.id
          ? { ...item, quantity: item.quantity + 1 }
          : item
      ));
    } else {
      setCart([...cart, { ticketId: ticket.id, quantity: 1, ticket }]);
    }
  };

  const removeFromCart = (ticketId) => {
    const existing = cart.find(item => item.ticketId === ticketId);
    if (existing.quantity > 1) {
      setCart(cart.map(item =>
        item.ticketId === ticketId
          ? { ...item, quantity: item.quantity - 1 }
          : item
      ));
    } else {
      setCart(cart.filter(item => item.ticketId !== ticketId));
    }
  };

  const calculateTotal = () => {
    return cart.reduce((sum, item) => sum + (parseFloat(item.ticket.price) * item.quantity), 0).toFixed(2);
  };

  const handleCheckout = async () => {
    if (cart.length === 0) {
      setError('Cart is empty');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const checkoutData = {
        items: cart.map(item => ({
          ticketId: item.ticketId,
          quantity: item.quantity
        })),
        paymentMethod: paymentMethod === 'CreditCard' ? 1 : 2
      };

      const response = await axios.post(`${API_URL}/orders/checkout`, checkoutData);
      
      setSuccess(response.data);
      setCart([]);
      fetchTickets(); // Refresh ticket availability
    } catch (err) {
      setError(err.response?.data?.message || 'Checkout failed');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="container success-screen">
        <div className="success-card">
          <div className="success-icon">✓</div>
          <h1>Payment Successful!</h1>
          <div className="success-details">
            <p><strong>Transaction ID:</strong> {success.transactionId}</p>
            <p><strong>Order Number:</strong> {success.orderNumber}</p>
            <p><strong>Total Amount:</strong> {success.totalAmount} AED</p>
            <p><strong>Timestamp:</strong> {new Date(success.completedAt).toLocaleString()}</p>
          </div>
          <button onClick={() => setSuccess(null)} className="btn-primary">
            Back to Tickets
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container">
      <header className="header">
        <h1>🎫 E-Ticketing System</h1>
        <p>Select your tickets and complete the checkout</p>
      </header>

      {error && (
        <div className="alert alert-error">
          {error}
          <button onClick={() => setError(null)}>×</button>
        </div>
      )}

      <div className="content">
        <div className="tickets-section">
          <h2>Available Tickets</h2>
          <div className="tickets-grid">
            {tickets.map(ticket => (
              <div key={ticket.id} className="ticket-card">
                <div className="ticket-header">
                  <h3>{ticket.name}</h3>
                  <span className={`ticket-badge badge-${getTicketTypeString(ticket.type).toLowerCase()}`}>
                    {getTicketTypeString(ticket.type)}
                  </span>
                </div>
                <div className="ticket-price">{ticket.price} AED</div>
                <div className="ticket-availability">
                  Available: {ticket.availableQuantity} / {ticket.initialQuantity}
                </div>
                <button
                  onClick={() => addToCart(ticket)}
                  disabled={ticket.availableQuantity === 0}
                  className="btn-add"
                >
                  {ticket.availableQuantity === 0 ? 'Sold Out' : 'Add to Cart'}
                </button>
              </div>
            ))}
          </div>
        </div>

        <div className="cart-section">
          <h2>Shopping Cart</h2>
          {cart.length === 0 ? (
            <p className="empty-cart">Your cart is empty</p>
          ) : (
            <>
              <div className="cart-items">
                {cart.map(item => (
                  <div key={item.ticketId} className="cart-item">
                    <div className="cart-item-info">
                      <strong>{item.ticket.name}</strong>
                      <span>{item.ticket.price} AED × {item.quantity}</span>
                    </div>
                    <div className="cart-item-actions">
                      <button onClick={() => removeFromCart(item.ticketId)} className="btn-remove">
                        −
                      </button>
                      <span className="quantity">{item.quantity}</span>
                      <button onClick={() => addToCart(item.ticket)} className="btn-add-small">
                        +
                      </button>
                    </div>
                  </div>
                ))}
              </div>

              <div className="cart-total">
                <strong>Total: {calculateTotal()} AED</strong>
              </div>

              <div className="payment-method">
                <h3>Payment Method</h3>
                <label className="radio-label">
                  <input
                    type="radio"
                    value="CreditCard"
                    checked={paymentMethod === 'CreditCard'}
                    onChange={(e) => setPaymentMethod(e.target.value)}
                  />
                  <span>💳 Credit Card (Instant)</span>
                </label>
                <label className="radio-label">
                  <input
                    type="radio"
                    value="QRScan"
                    checked={paymentMethod === 'QRScan'}
                    onChange={(e) => setPaymentMethod(e.target.value)}
                  />
                  <span>📱 QR Scan (8-second delay)</span>
                </label>
              </div>

              <button
                onClick={handleCheckout}
                disabled={loading}
                className="btn-checkout"
              >
                {loading ? 'Processing...' : 'Checkout'}
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
