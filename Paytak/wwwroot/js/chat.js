document.addEventListener('DOMContentLoaded', function() {
    const messageInput = document.getElementById('messageInput');
    const sendButton = document.getElementById('sendButton');
    const chatMessages = document.getElementById('chatMessages');
    const aiStatus = document.getElementById('aiStatus');

    // Send message on button click
    sendButton.addEventListener('click', sendMessage);

    // Send message on Enter key
    messageInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    function sendMessage() {
        const message = messageInput.value.trim();
        if (!message) return;

        // Add user message to chat
        addMessageToChat('user', message);
        messageInput.value = '';

        // Show loading indicator
        const loadingId = addLoadingMessage();
        updateAIStatus(true);

        // Send to API
        fetch('/api/chat/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                message: message,
                model: 'gpt-4o',
                maxTokens: 1000,
                temperature: 0.7
            })
        })
        .then(response => response.json())
        .then(data => {
            // Remove loading message
            removeLoadingMessage(loadingId);
            updateAIStatus(false);

            if (data.success) {
                // Add AI response to chat
                addMessageToChat('ai', data.response);
            } else {
                // Show error message
                addMessageToChat('error', 'Hata: ' + (data.errorMessage || 'Bilinmeyen hata'));
            }
        })
        .catch(error => {
            // Remove loading message
            removeLoadingMessage(loadingId);
            updateAIStatus(false);
            
            // Show error message
            addMessageToChat('error', 'Bağlantı hatası: ' + error.message);
        });
    }

    function addMessageToChat(type, content) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message-bubble ${type === 'user' ? 'user-message' : type === 'ai' ? 'ai-message' : 'error-message'}`;
        
        if (type === 'user') {
            messageDiv.innerHTML = `
                <div class="d-flex align-items-start">
                    <div class="ms-auto">
                        <div class="d-flex align-items-center mb-2">
                            <span class="me-2">👤</span>
                            <small class="text-light">Siz</small>
                        </div>
                        <div class="message-content">${content}</div>
                        <small class="text-light">${new Date().toLocaleTimeString()}</small>
                    </div>
                </div>
            `;
        } else if (type === 'ai') {
            messageDiv.innerHTML = `
                <div class="d-flex align-items-start">
                    <div class="me-3">
                        <i class="fas fa-robot text-success" style="font-size: 1.2rem;"></i>
                    </div>
                    <div>
                        <strong>Paitak</strong>
                        <div class="mt-1">
                            ${content}
                        </div>
                        <small class="text-light">${new Date().toLocaleTimeString()}</small>
                    </div>
                </div>
            `;
        } else {
            messageDiv.innerHTML = `
                <div class="d-flex align-items-start">
                    <div class="me-3">
                        <i class="fas fa-exclamation-triangle text-warning" style="font-size: 1.2rem;"></i>
                    </div>
                    <div>
                        <strong>Hata</strong>
                        <div class="mt-1">
                            ${content}
                        </div>
                    </div>
                </div>
            `;
        }
        
        chatMessages.appendChild(messageDiv);
        
        // Force scroll to bottom immediately and after a delay
        forceScrollToBottom();
        setTimeout(forceScrollToBottom, 100);
        setTimeout(forceScrollToBottom, 300);
        setTimeout(forceScrollToBottom, 500);
    }

    function forceScrollToBottom() {
        chatMessages.scrollTop = chatMessages.scrollHeight;
        chatMessages.scrollTo({
            top: chatMessages.scrollHeight,
            behavior: 'smooth'
        });
    }

    function addLoadingMessage() {
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'message-bubble ai-message';
        loadingDiv.id = 'loading-message';
        
        loadingDiv.innerHTML = `
            <div class="d-flex align-items-start">
                <div class="me-3">
                    <i class="fas fa-robot text-success" style="font-size: 1.2rem;"></i>
                </div>
                <div>
                    <strong>Paitak</strong>
                    <div class="mt-1 d-flex align-items-center">
                        <div class="loading-spinner me-3"></div>
                        Düşünüyor...
                    </div>
                </div>
            </div>
        `;
        
        chatMessages.appendChild(loadingDiv);
        
        // Force scroll to bottom immediately and after a delay
        forceScrollToBottom();
        setTimeout(forceScrollToBottom, 100);
        setTimeout(forceScrollToBottom, 300);
        
        return 'loading-message';
    }

    function removeLoadingMessage(loadingId) {
        const loadingElement = document.getElementById(loadingId);
        if (loadingElement) {
            loadingElement.remove();
        }
    }

    function updateAIStatus(isThinking) {
        if (aiStatus) {
            const statusText = aiStatus.parentElement.querySelector('small');
            if (statusText) {
                if (isThinking) {
                    aiStatus.style.display = 'block';
                    statusText.textContent = 'AI Düşünüyor...';
                } else {
                    aiStatus.style.display = 'none';
                    statusText.textContent = 'AI Hazır';
                }
            }
        }
    }
}); 